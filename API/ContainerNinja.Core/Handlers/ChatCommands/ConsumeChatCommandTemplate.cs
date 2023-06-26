using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "template" })]
    public class ConsumeChatCommandTemplate : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOTemplate>
    {
        public ChatAICommandDTOTemplate Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandTemplateHandler : IRequestHandler<ConsumeChatCommandTemplate, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandTemplateHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandTemplate model, CancellationToken cancellationToken)
        {
            //Command logic
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "template";
            return "Success";
        }
    }
}