using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "get_kitchen_inventory_products" })]
    public class ConsumeChatCommandGetKitchenInventory : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetKitchenInventory>
    {
        public ChatAICommandDTOGetKitchenInventory Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetKitchenInventoryHandler : IRequestHandler<ConsumeChatCommandGetKitchenInventory, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetKitchenInventoryHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetKitchenInventory model, CancellationToken cancellationToken)
        {
            var kitchenInventory = _repository.KitchenProducts.Set.Where(kp => kp.Amount > 0).ToList();
            var allInventoryArray = new JArray();
            foreach (var kitchenProduct in kitchenInventory)
            {
                var foundObject = new JObject();
                foundObject["KitchenProductId"] = kitchenProduct.Id;
                foundObject["KitchenProductName"] = kitchenProduct.Name;
                foundObject["Quantity"] = kitchenProduct.Amount;
                allInventoryArray.Add(foundObject);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "kitchen-products";
            return JsonConvert.SerializeObject(allInventoryArray);
        }
    }
}