using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using GNetServer;
using Newtonsoft.Json;
using System.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "add_user_to_server" })]
    public class ConsumeChatCommandAddUserToServer : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddUserToServer>
    {
        public ChatAICommandDTOAddUserToServer Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddUserToServerHandler : IRequestHandler<ConsumeChatCommandAddUserToServer, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandAddUserToServerHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandAddUserToServer model, CancellationToken cancellationToken)
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
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users" }));
            }

            gameServer.JoinServer(userId, model.Command.UserName);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{model.Command.UserName} joined server {gameServer.name}.";
        }
    }
}