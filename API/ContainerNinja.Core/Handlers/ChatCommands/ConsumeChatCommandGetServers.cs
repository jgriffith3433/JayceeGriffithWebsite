using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_servers" })]
    public class ConsumeChatCommandGetServers : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetServers>
    {
        public ChatAICommandDTOGetServers Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetServersHandler : IRequestHandler<ConsumeChatCommandGetServers, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetServersHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetServers model, CancellationToken cancellationToken)
        {
            var serverList = _gameService.GetGameServerList();
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var serverArray = new JArray();
            foreach(var server in serverList)
            {
                var serverObject = new JObject();
                serverObject["ServerName"] = server.Name;
                serverObject["ServerId"] = server.ServerId;
                //serverObject["PlayerCount"] = server.PlayerCount;
                //serverObject["GameId"] = server.GameId;
                serverObject["OwnerUserId"] = server.UserId;
                serverArray.Add(serverObject);
            }

            return JsonConvert.SerializeObject(serverArray);
        }
    }
}