using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_players" })]
    public class ConsumeChatCommandGetPlayers : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetPlayers>
    {
        public ChatAICommandDTOGetPlayers Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetPlayersHandler : IRequestHandler<ConsumeChatCommandGetPlayers, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetPlayersHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetPlayers model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);

            if (gameServer == null)
            {
                var systemMessage = $"Cannot find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var playerArray = new JArray();
            for (int i = 0; i < gameServer.players.size; ++i)
            {
                var player = gameServer.players.buffer[i];

                var playerObject = new JObject();
                playerObject["Id"] = player.id;
                playerObject["Name"] = player.name;
                playerArray.Add(playerObject);
            }

            return JsonConvert.SerializeObject(playerArray);
        }
    }
}