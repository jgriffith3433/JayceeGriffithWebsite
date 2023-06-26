using AutoMapper;
using ContainerNinja.Contracts.Data;
using MediatR;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.ViewModels;

namespace ContainerNinja.Core.Handlers.Queries
{
    public record GetChatTextFromSpeechQuery : IRequest<GetChatTextFromSpeechVm>
    {
        public string PreviousMessage { get; set; }
        public byte[] Speech { get; set; }
    }

    public class GetChatTextFromSpeechQueryHandler : IRequestHandler<GetChatTextFromSpeechQuery, GetChatTextFromSpeechVm>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IChatAIService _chatAIService;

        public GetChatTextFromSpeechQueryHandler(IUnitOfWork repository, IMapper mapper, ICachingService cache, IChatAIService chatAIService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _chatAIService = chatAIService;
        }

        public async Task<GetChatTextFromSpeechVm> Handle(GetChatTextFromSpeechQuery request, CancellationToken cancellationToken)
        {
            var speechToTextMessage = await _chatAIService.GetTextFromSpeech(request.Speech, request.PreviousMessage);

            return new GetChatTextFromSpeechVm
            {
                Text = speechToTextMessage
            };
        }
    }
}