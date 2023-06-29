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
    [ChatCommandModel(new [] { "get_users_in_server" })]
    public class ConsumeChatCommandGetUsersInServer : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetUsersInServer>
    {
        public ChatAICommandDTOGetUsersInServer Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetUsersInServerHandler : IRequestHandler<ConsumeChatCommandGetUsersInServer, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetUsersInServerHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetUsersInServer model, CancellationToken cancellationToken)
        {
            var gameServer = _gameService.GetGameServerByServerName(model.Command.ServerName);

            if (gameServer == null)
            {
                var systemMessage = $"Cannot find server by name {model.Command.ServerName}";
                throw new ChatAIException(systemMessage, JsonConvert.SerializeObject(new { name = "get_servers" }));
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var usersInServerArray = new JArray();
            for (int i = 0; i < gameServer.players.size; ++i)
            {
                var player = gameServer.players.buffer[i];

                var playerObject = new JObject();
                playerObject["UserName"] = player.name;
                usersInServerArray.Add(playerObject);
            }

            return JsonConvert.SerializeObject(usersInServerArray);
        }
    }
}