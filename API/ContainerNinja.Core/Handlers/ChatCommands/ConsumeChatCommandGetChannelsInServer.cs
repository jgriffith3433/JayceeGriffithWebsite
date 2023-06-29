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
    [ChatCommandModel(new [] { "get_channels_in_server" })]
    public class ConsumeChatCommandGetChannelsInServer : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetChannelsInServer>
    {
        public ChatAICommandDTOGetChannelsInServer Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetChannelsInServerHandler : IRequestHandler<ConsumeChatCommandGetChannelsInServer, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetChannelsInServerHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetChannelsInServer model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);

            if (gameServer == null)
            {
                var systemMessage = $"Cannot find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var channelsInServerArray = new JArray();
            var channels = new Dictionary<int, string>
            {
                { 1, "Chat" },
                { 2, "Table Tennis" },
                { 3, "RFCs" },
                { 4, "Object Creation" },
                { 5, "Frequent Packets" },
                { 6, "Movement" },
                { 7, "Multiple CHannels" },
                { 8, "Portfolio" },
            };

            for (var i = 1; i <= 8; i++)
            {
                var channelObject = new JObject();
                channelObject["ChannelId"] = i;
                channelObject["ChannelName"] = channels[i];
                channelsInServerArray.Add(channelObject);
            }

            return JsonConvert.SerializeObject(channelsInServerArray);

        }
    }
}