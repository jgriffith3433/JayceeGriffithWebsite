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
    //[ChatCommandModel(new[] { "add_kitchen_products_to_inventory" })]
    public class ConsumeChatCommandCreateKitchenProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOCreateKitchenProducts>
    {
        public ChatAICommandDTOCreateKitchenProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandCreateKitchenProductHandler : IRequestHandler<ConsumeChatCommandCreateKitchenProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandCreateKitchenProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandCreateKitchenProducts model, CancellationToken cancellationToken)
        {
            //then go through items that don't have an Id and get the kitchen product by name
            //if there are multiple results, ask the user which one they want to take stock of
            //if there is one record but the name is not an exact match then verify
            //if there are none then create a new kitchen product record
            var kitchenProductsToAdd = new List<KitchenProduct>();
            var kitchenProductsToUpdate = new List<KitchenProduct>();
            foreach (var item in model.Command.KitchenProducts)
            {
                var predicate = PredicateBuilder.New<KitchenProduct>();
                var searchTerms = string.Join(' ', item.KitchenProductName.ToLower().Split('-')).Split(' ');
                foreach (var searchTerm in searchTerms)
                {
                    predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
                    if (searchTerm[searchTerm.Length - 1] == 's')
                    {
                        predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm.Substring(0, searchTerm.Length - 1)));
                    }
                }

                var existingKitchenProductEntities = _repository.KitchenProducts.Set.AsExpandable().Where(predicate).ToList();
                if (existingKitchenProductEntities.Count == 0)
                {
                    var newKitchenProductEntity = _repository.KitchenProducts.CreateProxy();
                    {
                        newKitchenProductEntity.Name = item.KitchenProductName;
                        newKitchenProductEntity.Amount = item.Quantity;
                        newKitchenProductEntity.KitchenUnitType = item.KitchenUnitType;
                    }
                    kitchenProductsToAdd.Add(newKitchenProductEntity);
                }
                else if (existingKitchenProductEntities.Count == 1)
                {
                    if (existingKitchenProductEntities[0].Name.ToLower() == item.KitchenProductName.ToLower())
                    {
                        existingKitchenProductEntities[0].Amount = item.Quantity;
                        existingKitchenProductEntities[0].KitchenUnitType = item.KitchenUnitType;
                        kitchenProductsToUpdate.Add(existingKitchenProductEntities[0]);
                    }
                    else
                    {
                        var systemMessage = $"Record '{existingKitchenProductEntities[0].Name}' found for '{item.KitchenProductName}'. Is that the record you want to update or do you want to create a new one?\n";

                        var kitchenProductObject = new JObject();
                        kitchenProductObject["KitchenProductId"] = existingKitchenProductEntities[0].Id;
                        kitchenProductObject["KitchenProductName"] = existingKitchenProductEntities[0].Name;

                        systemMessage += JsonConvert.SerializeObject(kitchenProductObject);
                        throw new ChatAIException(systemMessage, "none");
                    }
                }
                else
                {
                    //TODO: Probably a way to one line this first block into a query
                    var exactMatch = existingKitchenProductEntities.FirstOrDefault(ps => ps.Name.ToLower() == item.KitchenProductName.ToLower());

                    if (exactMatch != null)
                    {
                        //check if there are no "too similar" matches
                        //for instance: "trail mix" could be "raisin free trail mix" or "trail mix snack"
                        var tooSimilarMatch = existingKitchenProductEntities.FirstOrDefault(ps => ps != exactMatch && ps.Name.ToLower().Contains(item.KitchenProductName.ToLower()));
                        if (tooSimilarMatch != null)
                        {
                            exactMatch = null;
                        }
                    }

                    if (exactMatch != null)
                    {
                        exactMatch.Amount = item.Quantity;
                        exactMatch.KitchenUnitType = item.KitchenUnitType;
                        kitchenProductsToUpdate.Add(exactMatch);
                    }
                    else
                    {
                        var systemMessage = $"Multiple records found for '{item.KitchenProductName}'. Did you mean to update any of these or create a new one?\n";
                        var multipleRecordsArray = new JArray();
                        foreach (var existingKitchenProductEntity in existingKitchenProductEntities)
                        {
                            var kitchenProductObject = new JObject();
                            kitchenProductObject["KitchenProductId"] = existingKitchenProductEntity.Id;
                            kitchenProductObject["KitchenProductName"] = existingKitchenProductEntity.Name;
                            multipleRecordsArray.Add(kitchenProductObject);
                        }

                        systemMessage += JsonConvert.SerializeObject(multipleRecordsArray);
                        throw new ChatAIException(systemMessage, "none");
                    }
                }
            }

            foreach (var newKitchenProduct in kitchenProductsToAdd)
            {
                _repository.KitchenProducts.Add(newKitchenProduct);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var inventoryModifiedObject = new JObject
            {
                { "Flow", "User is adding to their kitchen inventory" }
            };
            if (kitchenProductsToAdd.Count > 0)
            {
                var addedRecordsObject = new JObject
                {
                    { "Message", $"{kitchenProductsToAdd.Count} records have been added to the database" }
                };
                var addedArray = new JArray();
                foreach (var newKitchenProduct in kitchenProductsToAdd)
                {
                    var kitchenProductObject = new JObject();
                    kitchenProductObject["KitchenProductId"] = newKitchenProduct.Id;
                    kitchenProductObject["KitchenProductName"] = newKitchenProduct.Name;
                    kitchenProductObject["Quantity"] = newKitchenProduct.Amount;
                    kitchenProductObject["KitchenUnitType"] = newKitchenProduct.KitchenUnitType.ToString();
                    addedArray.Add(kitchenProductObject);
                }
                addedRecordsObject.Add("Results", addedArray);
                inventoryModifiedObject.Add("Added", addedRecordsObject);
            }
            if (kitchenProductsToUpdate.Count > 0)
            {
                var updatedRecordsObject = new JObject
                {
                    { "Updated", $"{kitchenProductsToUpdate.Count} records have been updated in the database" }
                };
                var updatedArray = new JArray();
                foreach (var existingKitchenProduct in kitchenProductsToUpdate)
                {
                    _repository.KitchenProducts.Update(existingKitchenProduct);
                    var kitchenProductObject = new JObject();
                    kitchenProductObject["KitchenProductId"] = existingKitchenProduct.Id;
                    kitchenProductObject["KitchenProductName"] = existingKitchenProduct.Name;
                    kitchenProductObject["Quantity"] = existingKitchenProduct.Amount;
                    kitchenProductObject["KitchenUnitType"] = existingKitchenProduct.KitchenUnitType.ToString();
                    updatedArray.Add(kitchenProductObject);
                }
                updatedRecordsObject.Add("Results", updatedArray);
                inventoryModifiedObject.Add("Updated", updatedRecordsObject);
            }

            model.Response.NavigateToPage = "kitchen-products";
            //model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(inventoryModifiedObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}