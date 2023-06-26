using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using System.Text;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using OpenAI.ObjectModels;
using ContainerNinja.Core.Handlers.ChatCommands;
using ContainerNinja.Core.Exceptions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatResponseQuery : IRequest<ChatResponseVM>
    {
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string CurrentUrl { get; set; }
        public bool NormalChat { get; set; }
        public string NavigateToPage { get; set; }
        public string ForceFunctionCall { get; set; }
        public bool Dirty { get; set; }
    }

    public class GetChatResponseQueryHandler : IRequestHandler<GetChatResponseQuery, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public GetChatResponseQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(GetChatResponseQuery model, CancellationToken cancellationToken)
        {
            var allRecipes = _repository.Recipes.Set.AsNoTracking().Select(r => new ChatRecipeVM { RecipeId = r.Id, RecipeName = r.Name }).ToList();
            var allKitchenProducts = _repository.KitchenProducts.Set.AsNoTracking().Select(r => new ChatKitchenProductVM { KitchenProductId = r.Id, KitchenProductName = r.Name, Quantity = r.Amount }).ToList();
            var allWalmartProducts = _repository.WalmartProducts.Set.AsNoTracking().Select(r => new ChatWalmartProductVM { WalmartProductId = r.Id, WalmartProductName = r.Name }).ToList();
            var chatResponseVM = new ChatResponseVM
            {
                ChatConversationId = model.ChatConversation.Id,
                ChatMessages = model.ChatMessages,
            };
            chatResponseVM.ChatMessages.ForEach(cm =>
            {
                if (string.IsNullOrEmpty(cm.Content))
                {
                    cm.Content = "...";
                }
            });

            try
            {
                var messageToHandle = chatResponseVM.ChatMessages.LastOrDefault();
                var previousMessageToHandle = chatResponseVM.ChatMessages.LastOrDefault(cm => cm != messageToHandle);
                if (messageToHandle == null)
                {
                    throw new Exception("No new messages");
                }
                messageToHandle.Received = true;

                if (messageToHandle.To == StaticValues.ChatMessageRoles.Assistant)
                {
                    if (messageToHandle.From == StaticValues.ChatMessageRoles.User)
                    {
                        var chatMessageVM = await _chatAIService.GetChatResponse(chatResponseVM.ChatMessages, model.ForceFunctionCall, allRecipes, allKitchenProducts, allWalmartProducts);
                        chatResponseVM.ChatMessages.Add(chatMessageVM);
                        chatResponseVM.ForceFunctionCall = "auto";
                    }
                    else if (messageToHandle.From == "function")
                    {
                        var chatMessageVM = await _chatAIService.GetChatResponse(chatResponseVM.ChatMessages, model.ForceFunctionCall, allRecipes, allKitchenProducts, allWalmartProducts);
                        chatResponseVM.ChatMessages.Add(chatMessageVM);
                        chatResponseVM.ForceFunctionCall = "auto";
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.System)
                    {
                        var chatMessageVM = await _chatAIService.GetChatResponse(chatResponseVM.ChatMessages, model.ForceFunctionCall, allRecipes, allKitchenProducts, allWalmartProducts);
                        chatResponseVM.ChatMessages.Add(chatMessageVM);
                        chatResponseVM.ForceFunctionCall = "auto";
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.Assistant)
                    {
                        //TODO: Implement ai to ai communication
                        throw new NotImplementedException();
                    }
                }
                else if (messageToHandle.To == "function")
                {
                    if (messageToHandle.From == StaticValues.ChatMessageRoles.Assistant)
                    {
                        chatResponseVM = await _mediator.Send(new ConsumeChatCommand
                        {
                            ChatConversation = model.ChatConversation,
                            ChatMessages = chatResponseVM.ChatMessages,
                            CurrentChatMessage = messageToHandle,
                            CurrentUrl = model.CurrentUrl,
                            NavigateToPage = model.NavigateToPage,
                            Dirty = model.Dirty,
                        });
                        //weird bug where the model doesn't like seeing messages from assistant to function
                        //chatResponseVM.ChatMessages.Remove(messageToHandle);
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.User)
                    {
                        //TODO: Implement user to function communication
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.System)
                    {
                        //TODO: Implement system to function communication
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == "function")
                    {
                        //TODO: Implement function to function communication
                        throw new NotImplementedException();
                    }
                }
                else if (messageToHandle.To == StaticValues.ChatMessageRoles.User)
                {
                    //currently these flows should not happen because the chat system is RESTful
                    //so all messages start from the user and return to the user over http and get marked as received
                    if (messageToHandle.From == StaticValues.ChatMessageRoles.User)
                    {
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.System)
                    {
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == "function")
                    {
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.Assistant)
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (messageToHandle.To == StaticValues.ChatMessageRoles.System)
                {
                    //TODO: create a chat model that can tell if the user accepts or declines
                    if (messageToHandle.From == StaticValues.ChatMessageRoles.User)
                    {
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.System)
                    {
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == "function")
                    {
                        throw new NotImplementedException();
                    }
                    else if (messageToHandle.From == StaticValues.ChatMessageRoles.Assistant)
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                _repository.ChangeTracker.Clear();
                model.ChatConversation.Error = FlattenException(ex);
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = ex.Message,
                    From = StaticValues.ChatMessageRoles.System,
                    To = StaticValues.ChatMessageRoles.User,
                });
                chatResponseVM.ForceFunctionCall = "auto";
                _repository.ChatConversations.Update(model.ChatConversation);
                await _repository.CommitAsync();
            }
            catch (Exception ex)
            {
                model.ChatConversation.Error = FlattenException(ex);
                chatResponseVM.ChatMessages.Add(new ChatMessageVM
                {
                    Content = ex.Message,
                    From = StaticValues.ChatMessageRoles.System,
                    To = StaticValues.ChatMessageRoles.User,
                });
                chatResponseVM.ForceFunctionCall = "auto";
                //chatResponseVM.Error = true;
            }
            _cache.Clear();

            model.ChatConversation.Content = JsonSerializer.Serialize(model);
            _repository.ChatConversations.Update(model.ChatConversation);

            await _repository.CommitAsync();

            return chatResponseVM;
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