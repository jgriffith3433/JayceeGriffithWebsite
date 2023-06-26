using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "create_new_order" })]
    public class ConsumeChatCommandCreateOrder : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateOrder>
    {
        public ChatAICommandDTOCreateOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateOrderHandler : IRequestHandler<ConsumeChatCommandCreateOrder, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateOrder model, CancellationToken cancellationToken)
        {
            var orderEntity = _repository.Orders.CreateProxy();
            _repository.Orders.Add(orderEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var orderObject = new JObject();
            orderObject["OrderId"] = orderEntity.Id;
            var orderItemsArray = new JArray();
            orderObject["OrderItems"] = orderItemsArray;
            model.Response.NavigateToPage = "orders";
            return JsonConvert.SerializeObject(orderObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}