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
    [ChatCommandModel(new [] { "get_users" })]
    public class ConsumeChatCommandGetUsers : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetUsers>
    {
        public ChatAICommandDTOGetUsers Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetUsersHandler : IRequestHandler<ConsumeChatCommandGetUsers, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetUsersHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetUsers model, CancellationToken cancellationToken)
        {
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var userArray = new JArray();
            foreach(var userKvp in _gameService.Users)
            {
                var userObject = new JObject();
                userObject["UserId"] = userKvp.Key;
                userObject["UserName"] = userKvp.Value;
                userArray.Add(userObject);
            }

            return JsonConvert.SerializeObject(userArray);
        }
    }
}