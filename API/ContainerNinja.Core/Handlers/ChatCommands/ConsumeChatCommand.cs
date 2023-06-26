using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;
using System.Text;
using ContainerNinja.Contracts.ViewModels;
using OpenAI.ObjectModels;
using System.Reflection;
using ContainerNinja.Core.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using System.Dynamic;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Common;
using Newtonsoft.Json.Linq;
using OpenAI.ObjectModels.ResponseModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    public class ConsumeChatCommand : IRequest<ChatResponseVM>
    {
        public ChatConversation ChatConversation { get; set; }
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatMessageVM? CurrentChatMessage { get; set; }
        public string CurrentUrl { get; set; }
        public string NavigateToPage { get; set; }
        public bool Dirty { get; set; }
    }

    public class ConsumeChatCommandHandler : IRequestHandler<ConsumeChatCommand, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;
        private readonly IMediator _mediator;

        public ConsumeChatCommandHandler(IUnitOfWork repository, ICachingService cache, IMediator mediator)
        {
            _repository = repository;
            _cache = cache;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(ConsumeChatCommand request, CancellationToken cancellationToken)
        {
            var functionCallObject = JsonConvert.DeserializeObject<ChatAICommandDTO>(request.CurrentChatMessage.FunctionCall);

            var chatCommandEntity = _repository.ChatCommands.CreateProxy();
            {
                chatCommandEntity.RawChatAICommand = JsonConvert.SerializeObject(request.CurrentChatMessage, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                chatCommandEntity.CurrentUrl = request.CurrentUrl;
                chatCommandEntity.CommandName = functionCallObject.Name;
                chatCommandEntity.ChatConversationId = request.ChatConversation.Id;
            };
            _repository.ChatCommands.Add(chatCommandEntity);
            request.ChatConversation.ChatCommands.Add(chatCommandEntity);
            await _repository.CommitAsync();
            var chatResponseVM = new ChatResponseVM
            {
                ChatConversationId = request.ChatConversation.Id,
                ChatMessages = request.ChatMessages,
                NavigateToPage = request.NavigateToPage,
                Dirty = request.Dirty,
            };
            try
            {
                if (!string.IsNullOrEmpty(functionCallObject.Name))
                {
                    var chatCommandConsumerType = GetChatCommandConsumerType(string.Join("_", functionCallObject.Name.ToLower().Split(" ")));
                    if (chatCommandConsumerType != null)
                    {
                        var chatCommandConsumer = Activator.CreateInstance(chatCommandConsumerType);
                        if (chatCommandConsumer != null)
                        {
                            object? functionCallArgumentsObject = null;
                            try
                            {
                                var chatCommandModelType = GetChatAICommandModelTypeFromConsumerType(chatCommandConsumerType);
                                var jObjectToCheckSchemaValidation = JsonConvert.DeserializeObject<JObject>(functionCallObject.Arguments);
                                var ccs = chatCommandModelType.GetCustomAttribute<ChatCommandSpecification>();
                                if (ccs == null)
                                {
                                    throw new NotImplementedException(typeof(ChatCommandSpecification).Name + " not found on type " + chatCommandModelType.Name);
                                }
                                if (ccs.IsValidAgainstSchema(jObjectToCheckSchemaValidation, chatCommandModelType) == false)
                                {
                                    throw new ApiValidationException(ccs.GetValidationErrors().Select(v => new FluentValidation.Results.ValidationFailure(v.Path, v.Message, v.Value)));
                                }
                                functionCallArgumentsObject = JsonConvert.DeserializeObject(functionCallObject.Arguments, chatCommandModelType);
                            }
                            catch(ApiValidationException ex)
                            {
                                throw ex;
                            }
                            catch (Exception ex)
                            {
                                throw new ChatAIException(ex.Message, JsonConvert.SerializeObject(new { name = functionCallObject.Name }));
                            }

                            chatCommandConsumerType.GetProperty("Command")?.SetValue(chatCommandConsumer, functionCallArgumentsObject);

                            chatCommandConsumerType.GetProperty("Response")?.SetValue(chatCommandConsumer, chatResponseVM);

                            var result = await _mediator.Send(chatCommandConsumer, cancellationToken) as string;
                            request.CurrentChatMessage.Content = result;
                            request.CurrentChatMessage.FunctionCall = null;
                            request.CurrentChatMessage.Name = functionCallObject.Name;
                            request.CurrentChatMessage.To = StaticValues.ChatMessageRoles.Assistant;
                            request.CurrentChatMessage.From = "function";
                            request.CurrentChatMessage.Received = false;
                            //chatResponseVM.ChatMessages.Add(new ChatMessageVM
                            //{
                            //    Content = result,
                            //    From = "function",
                            //    To = StaticValues.ChatMessageRoles.Assistant,
                            //    Name = chatAICommandName,
                            //});
                            if (string.IsNullOrEmpty(chatResponseVM.ForceFunctionCall))
                            {
                                chatResponseVM.ForceFunctionCall = "auto";
                            }
                        }
                    }
                    else
                    {
                        throw new ChatAIException("Unknown command: " + functionCallObject.Name);
                    }
                }
                chatCommandEntity.ChangedData = chatResponseVM.Dirty;
                chatCommandEntity.UnknownCommand = chatResponseVM.UnknownCommand;
                chatCommandEntity.NavigateToPage = chatResponseVM.NavigateToPage;
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (ApiValidationException ex)
            {
                var errors = "";

                foreach (var error in ex.Errors)
                {
                    var validationMessage = "";
                    foreach (var v in error.Value)
                    {
                        if (v.Contains("ForceFunctionCall="))
                        {
                            validationMessage += "More information required";
                        }
                        else
                        {
                            validationMessage += " " + v;
                        }
                    }
                    if (!string.IsNullOrEmpty(validationMessage.Trim()))
                    {
                        errors += "\n" + validationMessage.Trim();
                    }
                }

                chatCommandEntity.Error = FlattenException(ex) + errors;
                request.CurrentChatMessage.Content = JsonConvert.SerializeObject(new { Response = errors });
                request.CurrentChatMessage.FunctionCall = null;
                request.CurrentChatMessage.Name = functionCallObject.Name;
                request.CurrentChatMessage.To = StaticValues.ChatMessageRoles.Assistant;
                request.CurrentChatMessage.From = "function";
                request.CurrentChatMessage.Received = false;
                chatResponseVM.ForceFunctionCall = ex.ForceFunctionCall;
                //chatResponseVM.ChatMessages.Add(new ChatMessageVM
                //{
                //    Content = string.IsNullOrEmpty(errors) ? "More information required" : "Validation:\n" + errors,
                //    From = StaticValues.ChatMessageRoles.System,
                //    To = StaticValues.ChatMessageRoles.Assistant,
                //    Name = chatAICommandName,
                //});
                request.ChatConversation.Content = JsonConvert.SerializeObject(request.ChatMessages, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (ChatAIException ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                request.CurrentChatMessage.Content = JsonConvert.SerializeObject(new { Response = ex.Message });
                request.CurrentChatMessage.FunctionCall = null;
                request.CurrentChatMessage.Name = functionCallObject.Name;
                request.CurrentChatMessage.To = StaticValues.ChatMessageRoles.Assistant;
                request.CurrentChatMessage.From = "function";
                request.CurrentChatMessage.Received = false;
                chatResponseVM.ForceFunctionCall = ex.ForceFunctionCall;
                //chatResponseVM.ChatMessages.Add(new ChatMessageVM
                //{
                //    Content = ex.Message,
                //    From = StaticValues.ChatMessageRoles.System,
                //    To = StaticValues.ChatMessageRoles.Assistant,
                //    Name = chatAICommandName,
                //});
                request.ChatConversation.Content = JsonConvert.SerializeObject(request.ChatMessages, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                _repository.ChangeTracker.Clear();
                chatCommandEntity.Error = FlattenException(ex);
                request.CurrentChatMessage.Content = JsonConvert.SerializeObject(new { Response = chatCommandEntity.Error });
                request.CurrentChatMessage.FunctionCall = null;
                request.CurrentChatMessage.Name = functionCallObject.Name;
                request.CurrentChatMessage.To = StaticValues.ChatMessageRoles.Assistant;
                request.CurrentChatMessage.From = "function";
                request.CurrentChatMessage.Received = false;
                chatResponseVM.ForceFunctionCall = "auto";
                //chatResponseVM.ChatMessages.Add(new ChatMessageVM
                //{
                //    Content = chatCommandEntity.Error,
                //    From = "function",
                //    To = StaticValues.ChatMessageRoles.Assistant,
                //    Name = chatAICommandName,
                //});
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            catch (Exception ex)
            {
                chatCommandEntity.Error = FlattenException(ex);
                request.CurrentChatMessage.Content = JsonConvert.SerializeObject(new { Response = chatCommandEntity.Error });
                request.CurrentChatMessage.FunctionCall = null;
                request.CurrentChatMessage.Name = functionCallObject.Name;
                request.CurrentChatMessage.To = StaticValues.ChatMessageRoles.Assistant;
                request.CurrentChatMessage.From = "function";
                request.CurrentChatMessage.Received = false;
                chatResponseVM.ForceFunctionCall = "auto";
                //chatResponseVM.ChatMessages.Add(new ChatMessageVM
                //{
                //    Content = chatCommandEntity.Error,
                //    From = "function",
                //    To = StaticValues.ChatMessageRoles.Assistant,
                //    Name = chatAICommandName,
                //});
                request.ChatConversation.Content = JsonConvert.SerializeObject(chatResponseVM.ChatMessages, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                _repository.ChatCommands.Update(chatCommandEntity);
                await _repository.CommitAsync();
            }
            _cache.Clear();

            return chatResponseVM;
        }

        public static Type? GetChatCommandConsumerType(string commandName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.GetCustomAttribute<ChatCommandModel>() != null && x.GetCustomAttribute<ChatCommandModel>().CommandNames.Contains(commandName)).FirstOrDefault();
        }

        public static Type? GetChatAICommandModelTypeFromConsumerType(Type consumerType)
        {
            return consumerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IChatCommandConsumer<>)).SelectMany(i => i.GetGenericArguments()).FirstOrDefault();
        }

        private string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }
    }
}