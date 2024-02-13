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
    [ChatCommandModel(new [] { "bomb_user_in_channel" })]
    public class ConsumeChatCommandBombUserInChannel : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOBombUserInChannel>
    {
        public ChatAICommandDTOBombUserInChannel Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandBombUserInChannelHandler : IRequestHandler<ConsumeChatCommandBombUserInChannel, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandBombUserInChannelHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandBombUserInChannel model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);
            if (gameServer == null)
            {
                var systemMessage = $"Could not find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            var playerToBomb = gameServer.GetPlayerByName(model.Command.UserToBomb);
            if (playerToBomb == null)
            {
                var systemMessage = $"Could not find user by name {model.Command.UserToBomb}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_in_server" }));
            }

            var channel = playerToBomb.GetChannel(2);
            if (channel == null)
            {
                var systemMessage = $"User is not in channel 2";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_channels" }));
            }

            var objs = new object[2]
            {
                playerToBomb.id,
                playerToBomb.id,
            };

            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)0);
                bw.Write("CreateRocket");
                bw.Write("rocket");
                bw.WriteArray(objs);
            }
            gameServer.CreateObject(playerToBomb.ConnectionId, channel.id, playerToBomb.id, false, ms.GetBuffer());

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{model.Command.UserToBomb} is now being bombed";
        }
    }
}