using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using System.Text.Json;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using System.Text;
using OpenAI.ObjectModels;
using FluentValidation;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetNormalChatResponseQuery : IRequest<ChatResponseVM>
    {
        public List<ChatMessageVM> ChatMessages { get; set; }
        public ChatConversation ChatConversation { get; set; }
        public string CurrentUrl { get; set; }
    }

    public class GetNormalChatResponseQueryHandler : IRequestHandler<GetNormalChatResponseQuery, ChatResponseVM>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public GetNormalChatResponseQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator, IValidator<GetChatResponseQuery> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        public async Task<ChatResponseVM> Handle(GetNormalChatResponseQuery model, CancellationToken cancellationToken)
        {
            var chatResponseVM = new ChatResponseVM
            {
                ChatConversationId = model.ChatConversation.Id,
                ChatMessages = model.ChatMessages,
            };

            try
            {
                var messageToHandle = chatResponseVM.ChatMessages.LastOrDefault(cm => cm.Received == false);
                if (messageToHandle == null)
                {
                    throw new Exception("No new messages");
                }

                if (messageToHandle.To == StaticValues.ChatMessageRoles.Assistant)
                {
                    if (messageToHandle.From == StaticValues.ChatMessageRoles.User)
                    {
                        messageToHandle.Received = true;
                        var chatMessageVM = await _chatAIService.GetNormalChatResponse(chatResponseVM.ChatMessages);
                        chatResponseVM.ChatMessages.Add(chatMessageVM);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                model.ChatConversation.Error = FlattenException(e);
                chatResponseVM.Error = true;
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