using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatConversation : IRequest<ChatConversation>
    {
        public int ChatConversationId { get; set; }
    }

    public class GetChatConversationHandler : IRequestHandler<GetChatConversation, ChatConversation>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;
        private readonly IMediator _mediator;

        public GetChatConversationHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
            _mediator = mediator;
        }

        private async Task<ChatConversation> GetChatConversationEntity(int chatConversationId = -1)
        {
            ChatConversation chatConversationEntity;
            if (chatConversationId != -1)
            {
                chatConversationEntity = _repository.ChatConversations.Set.FirstOrDefault(cc => cc.Id == chatConversationId);
                if (chatConversationEntity == null)
                {
                    throw new Exception("Not Found");
                }
            }
            else
            {
                chatConversationEntity = _repository.ChatConversations.CreateProxy();
                {
                    chatConversationEntity.Content = "New";
                };
                _repository.ChatConversations.Add(chatConversationEntity);
            }

            await _repository.CommitAsync();
            return chatConversationEntity;
        }

        public async Task<ChatConversation> Handle(GetChatConversation request, CancellationToken cancellationToken)
        {
            return await GetChatConversationEntity(request.ChatConversationId);
        }
    }
}