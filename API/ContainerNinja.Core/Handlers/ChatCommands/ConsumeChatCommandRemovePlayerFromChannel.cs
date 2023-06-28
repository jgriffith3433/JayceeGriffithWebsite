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
    [ChatCommandModel(new [] { "remove_player_from_channel" })]
    public class ConsumeChatCommandRemovePlayerFromChannel : IRequest<string>, IChatCommandConsumer<ChatAICommandDTORemovePlayerFromChannel>
    {
        public ChatAICommandDTORemovePlayerFromChannel Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandRemovePlayerFromChannelHandler : IRequestHandler<ConsumeChatCommandRemovePlayerFromChannel, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandRemovePlayerFromChannelHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandRemovePlayerFromChannel model, CancellationToken cancellationToken)
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
            gameServer.LeaveChannel(player.ConnectionId, model.Command.ChannelId);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{player.name} removed from channel {model.Command.ChannelId}.";
        }
    }
}