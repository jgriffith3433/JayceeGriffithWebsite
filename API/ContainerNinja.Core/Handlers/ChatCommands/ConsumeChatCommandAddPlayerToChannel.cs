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
    [ChatCommandModel(new [] { "add_player_to_channel" })]
    public class ConsumeChatCommandAddPlayerToChannel : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddPlayerToChannel>
    {
        public ChatAICommandDTOAddPlayerToChannel Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddPlayerToChannelHandler : IRequestHandler<ConsumeChatCommandAddPlayerToChannel, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandAddPlayerToChannelHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandAddPlayerToChannel model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);
            if (gameServer == null)
            {
                var systemMessage = $"Could not find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var player = gameServer.GetPlayerByName(model.Command.PlayerName);
            if (player == null)
            {
                var systemMessage = $"Could not find player by name {model.Command.PlayerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_players" }));
            }

            var levelName = "";
            if (model.Command.ChannelId == 1)
            {
                levelName = "Portfolio";
            }
            else if (model.Command.ChannelId == 2)
            {
                levelName = "Table Tennis";
            }
            gameServer.JoinChannel(player.ConnectionId, model.Command.ChannelId, "", levelName, false, 10);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{player.name} added to channel {model.Command.ChannelId}.";
        }
    }
}