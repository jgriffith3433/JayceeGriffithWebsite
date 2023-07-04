using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using GNetServer;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Common;
using Microsoft.VisualBasic;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "spawn_objects_in_channel" })]
    public class ConsumeChatCommandSpawnObjectsInChannel : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSpawnObjectsInChannel>
    {
        public ChatAICommandDTOSpawnObjectsInChannel Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSpawnObjectsInChannelHandler : IRequestHandler<ConsumeChatCommandSpawnObjectsInChannel, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandSpawnObjectsInChannelHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandSpawnObjectsInChannel model, CancellationToken cancellationToken)
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

            var channel = player.GetChannel(model.Command.ChannelId);
            if (channel == null)
            {
                var systemMessage = $"User is not in channel {model.Command.ChannelId}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_users_channels" }));
            }

            foreach (var objectToSpawn in model.Command.ObjectsToSpawn)
            {
                if (!ObjectNames.SpawnableObjects.Contains(objectToSpawn.Name.ToLower()))
                {
                    var systemMessage = $"Invalid object name '{objectToSpawn.Name}'";
                    throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_spawnable_objects" }));
                }

                if (objectToSpawn.Quantity == 0)
                {
                    var systemMessage = $"Invalid quantity: 0";
                    throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "spawn_objects_in_channel" }));
                }
            }

            foreach (var objectToSpawn in model.Command.ObjectsToSpawn)
            {
                var row = 0;
                var column = 0;
                for (var i = 0; i < objectToSpawn.Quantity; i++)
                {
                    float xOffset = row * 2f;
                    float zOffset = column * 2f;
                    var objs = new object[3]
                    {
                        player.id,
                        xOffset,
                        zOffset,
                    };

                    var ms = new MemoryStream();
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write((byte)0);
                        bw.Write("CreateSandboxObject");
                        bw.Write(objectToSpawn.Name);
                        bw.WriteArray(objs);
                    }

                    gameServer.CreateObject(player.ConnectionId, channel.id, player.id, false, ms.GetBuffer());
                    if (i % 10 == 0)
                    {
                        column++;
                        row = 0;
                    }
                    else
                    {
                        row++;
                    }
                }
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";
            return $"{model.Command.ObjectsToSpawn.Count} objects added to channel {model.Command.ChannelId} for {model.Command.UserName}";
        }
    }
}