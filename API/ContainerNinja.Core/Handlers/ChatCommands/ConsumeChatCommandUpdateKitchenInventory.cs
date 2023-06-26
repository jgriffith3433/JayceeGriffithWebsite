using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using LinqKit;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "update_kitchen_product_quantities" })]
    public class ConsumeChatCommandUpdateKitchenInventory : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateKitchenInventory>
    {
        public ChatAICommandDTOUpdateKitchenInventory Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandUpdateKitchenProductHandler : IRequestHandler<ConsumeChatCommandUpdateKitchenInventory, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandUpdateKitchenProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUpdateKitchenInventory model, CancellationToken cancellationToken)
        {
            var kitchenProductsToUpdate = new List<KitchenProduct>();
            //first go through all the Ids we have and check if they all exist
            foreach (var item in model.Command.KitchenProducts)
            {
                if (item.KitchenProductId.HasValue)
                {
                    var existingKitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(ps => ps.Id == item.KitchenProductId);
                    if (existingKitchenProductEntity == null)
                    {
                        throw new ChatAIException($"Could not find kitchen product by ID: {item.KitchenProductId}", @"{ ""name"": ""search_kitchen_products"" }");
                    }
                    existingKitchenProductEntity.Amount = item.Quantity;
                    kitchenProductsToUpdate.Add(existingKitchenProductEntity);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            foreach (var existingKitchenProduct in kitchenProductsToUpdate)
            {
                _repository.KitchenProducts.Update(existingKitchenProduct);
            }

            var updatedObject = new JObject();
            if (kitchenProductsToUpdate.Count > 0)
            {
                var updatedRecordsObject = new JObject
                {
                    { "Message", $"{kitchenProductsToUpdate.Count} records have been updated in the database" }
                };
                var updatedArray = new JArray();
                foreach (var existingKitchenProduct in kitchenProductsToUpdate)
                {
                    var kitchenProductObject = new JObject();
                    kitchenProductObject["KitchenProductId"] = existingKitchenProduct.Id;
                    kitchenProductObject["KitchenProductName"] = existingKitchenProduct.Name;
                    kitchenProductObject["Quantity"] = existingKitchenProduct.Amount;
                    kitchenProductObject["KitchenUnitType"] = existingKitchenProduct.KitchenUnitType.ToString();
                    updatedArray.Add(kitchenProductObject);
                }
                updatedRecordsObject.Add("Results", updatedArray);
                updatedObject.Add("Updated", updatedRecordsObject);
            }

            model.Response.NavigateToPage = "kitchen-products";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(updatedObject);
        }
    }
}