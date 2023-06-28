using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using GNetServer;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "start_server" })]
    public class ConsumeChatCommandStartServer : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOStartServer>
    {
        public ChatAICommandDTOStartServer Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandStartServerHandler : IRequestHandler<ConsumeChatCommandStartServer, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandStartServerHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandStartServer model, CancellationToken cancellationToken)
        {
            var gameServersList = _gameService.GetGameServerList();
            var gameServerInfo = gameServersList.FirstOrDefault(gs => gs.Name.ToLower() == model.Command.ServerName.ToLower());

            if (gameServerInfo != null)
            {
                var systemMessage = $"Game server already exists with name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var ownerUserId = _gameService.GetUserIdByUserName(model.Command.OwnerUserName);

            if (ownerUserId == null)
            {
                var systemMessage = $"Could not find user by user name {model.Command.OwnerUserName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users" }));
            }

            var newGameServerInfo = new GameServerInfo(Guid.NewGuid().ToString().Substring(0, 4), GameServer.GameId);
            newGameServerInfo.Name = model.Command.ServerName;
            newGameServerInfo.PlayerCount = 0;
            newGameServerInfo.UserId = ownerUserId;

            _gameService.StopAnyUserServer(ownerUserId);
            _gameService.AddNewUserGameServer(ownerUserId, newGameServerInfo);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"Started new game server {newGameServerInfo.ServerId}";
        }
    }
}