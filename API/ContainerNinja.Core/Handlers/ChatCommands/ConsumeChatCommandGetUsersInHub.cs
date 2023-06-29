using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_users_in_hub" })]
    public class ConsumeChatCommandGetUsersInHub : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetUsersInHub>
    {
        public ChatAICommandDTOGetUsersInHub Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetUsersInHubHandler : IRequestHandler<ConsumeChatCommandGetUsersInHub, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetUsersInHubHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetUsersInHub model, CancellationToken cancellationToken)
        {
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var userArray = new JArray();
            foreach(var userKvp in _gameService.Users)
            {
                var userObject = new JObject();
                userObject["UserName"] = userKvp.Value;
                userArray.Add(userObject);
            }

            return JsonConvert.SerializeObject(userArray);
        }
    }
}