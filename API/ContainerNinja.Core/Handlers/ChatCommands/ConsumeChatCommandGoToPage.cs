using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "go_to_page" })]
    public class ConsumeChatCommandGoToPage : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGoToPage>
    {
        public ChatAICommandDTOGoToPage Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGoToPageHandler : IRequestHandler<ConsumeChatCommandGoToPage, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGoToPageHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGoToPage model, CancellationToken cancellationToken)
        {
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = string.Join('-', model.Command.Page.Split(' ')).ToLower();
            return $"Navigation complete: The user is now on the {model.Command.Page} page.";
        }
    }
}