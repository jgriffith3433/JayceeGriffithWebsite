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
    [ChatCommandModel(new[] { "add_user_to_channel" })]
    public class ConsumeChatCommandAddUserToChannel : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddUserToChannel>
    {
        public ChatAICommandDTOAddUserToChannel Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddUserToChannelHandler : IRequestHandler<ConsumeChatCommandAddUserToChannel, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandAddUserToChannelHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandAddUserToChannel model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);
            if (gameServer == null)
            {
                var systemMessage = $"Could not find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var player = gameServer.GetPlayerByName(model.Command.UserName);
            if (player == null)
            {
                var systemMessage = $"Could not find user by name {model.Command.UserName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_in_server" }));
            }

            if (player.IsInChannel(model.Command.ChannelId))
            {
                var systemMessage = $"User already in channel {model.Command.ChannelId}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_channels" }));
            }
            var additive = model.Command.StayInChannels && player.channels.size > 0;
            if (model.Command.StayInChannels == false)
            {
                var channelsToLeave = new List<Channel>();
                for (int i = 0; i < player.channels.size; ++i)
                {
                    channelsToLeave.Add(player.channels.buffer[i]);
                }
                foreach (var channel in channelsToLeave)
                {
                    gameServer.LeaveChannel(player.ConnectionId, channel.id);
                }
            }

            var levelName = "";
            switch (model.Command.ChannelId)
            {
                case 1:
                    levelName = "Chat";
                    break;
                case 2:
                    levelName = "Table Tennis";
                    break;
                case 3:
                    levelName = "RFCs";
                    break;
                case 4:
                    levelName = "Object Creation";
                    break;
                case 5:
                    levelName = "Frequent Packets";
                    break;
                case 6:
                    levelName = "Movement";
                    break;
                case 7:
                    levelName = "Multiple Channels";
                    break;
                case 8:
                    levelName = "Portfolio";
                    break;
                default:
                    var systemMessage = $"{model.Command.ChannelId} is not a valid channel.";
                    throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_channels_in_server" }));
            }
            gameServer.JoinChannel(player.ConnectionId, model.Command.ChannelId, "", levelName, false, 10, additive);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{player.name} added to channel {model.Command.ChannelId}.";
        }
    }
}