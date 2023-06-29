using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "remove_user_from_server" })]
    public class ConsumeChatCommandRemoveUserFromServer : IRequest<string>, IChatCommandConsumer<ChatAICommandDTORemoveUserFromServer>
    {
        public ChatAICommandDTORemoveUserFromServer Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandRemoveUserFromServerHandler : IRequestHandler<ConsumeChatCommandRemoveUserFromServer, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandRemoveUserFromServerHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandRemoveUserFromServer model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);
            if (gameServer == null)
            {
                var systemMessage = $"Could not find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var userId = _gameService.GetUserIdByUserName(model.Command.UserName);

            if (string.IsNullOrEmpty(userId))
            {
                var systemMessage = $"Could not find user by name {model.Command.UserName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_in_server" }));
            }

            gameServer.LeaveServer(userId);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{model.Command.UserName} joined server {gameServer.name}.";
        }
    }
}