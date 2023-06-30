using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContainerNinja.Contracts.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "get_spawnable_objects" })]
    public class ConsumeChatCommandGetSpawnableObjects : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetSpawnableObjects>
    {
        public ChatAICommandDTOGetSpawnableObjects Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetSpawnableObjectsHandler : IRequestHandler<ConsumeChatCommandGetSpawnableObjects, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IGameService _gameService;

        public ConsumeChatCommandGetSpawnableObjectsHandler(IUnitOfWork repository, IGameService gameService)
        {
            _repository = repository;
            _gameService = gameService;
        }

        public async Task<string> Handle(ConsumeChatCommandGetSpawnableObjects model, CancellationToken cancellationToken)
        {
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            //model.Response.NavigateToPage = "template";

            var channelsInServerArray = new JArray();
            foreach(var spawnableObjectName in ObjectNames.SpawnableObjects)
            {
                channelsInServerArray.Add(spawnableObjectName);
            }

            return JsonConvert.SerializeObject(channelsInServerArray);

        }
    }
}