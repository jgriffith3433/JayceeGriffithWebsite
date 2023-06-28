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
    [ChatCommandModel(new [] { "stop_server" })]
    public class ConsumeChatCommandStopServer : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOStopServer>
    {
        public ChatAICommandDTOStopServer Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandStopServerHandler : IRequestHandler<ConsumeChatCommandStopServer, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandStopServerHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandStopServer model, CancellationToken cancellationToken)
        {
            var gameServersList = _gameService.GetGameServerList();
            var gameServerInfo = gameServersList.FirstOrDefault(gs => gs.Name.ToLower() == model.Command.ServerName.ToLower());

            if (gameServerInfo == null)
            {
                var systemMessage = $"Could not find server info by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var gameServer = _gameService.GetGameServerByServerId(gameServerInfo.ServerId);
            if (gameServer == null)
            {
                var systemMessage = $"Could not find server by id {gameServerInfo.ServerId}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            gameServer.Stop();

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return "Stopped";
        }
    }
}