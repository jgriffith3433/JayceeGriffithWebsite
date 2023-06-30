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
    [ChatCommandModel(new [] { "get_users_channels" })]
    public class ConsumeChatCommandGetUsersChannels : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetUsersChannels>
    {
        public ChatAICommandDTOGetUsersChannels Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetUsersChannelsHandler : IRequestHandler<ConsumeChatCommandGetUsersChannels, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetUsersChannelsHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetUsersChannels model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);

            if (gameServer == null)
            {
                var systemMessage = $"Cannot find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var player = gameServer.GetPlayerByName(model.Command.UserName);
            if (player == null)
            {
                var systemMessage = $"Could not find user by name {model.Command.UserName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_in_server" }));
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var channels = new Dictionary<int, string>
            {
                { 1, "Chat" },
                { 2, "Sandbox" },
                { 3, "Portfolio" },
            };

            var usersChannelsArray = new JArray();
            for (int i = 0; i < player.channels.size; ++i)
            {
                var channel = player.channels.buffer[i];

                var channelObject = new JObject();
                channelObject["ChannelId"] = channel.id;
                channelObject["ChannelName"] = channels[channel.id];
                usersChannelsArray.Add(channelObject);
            }

            return JsonConvert.SerializeObject(usersChannelsArray);
        }
    }
}