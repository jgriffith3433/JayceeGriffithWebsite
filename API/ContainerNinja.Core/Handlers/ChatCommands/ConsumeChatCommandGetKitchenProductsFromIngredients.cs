using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_kitchen_products_from_ingredients" })]
    public class ConsumeChatCommandGetKitchenProductsFromIngredients : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetKitchenProductsFromIngredients>
    {
        public ChatAICommandDTOGetKitchenProductsFromIngredients Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetKitchenProductsFromIngredientsHandler : IRequestHandler<ConsumeChatCommandGetKitchenProductsFromIngredients, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetKitchenProductsFromIngredientsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetKitchenProductsFromIngredients model, CancellationToken cancellationToken)
        {
            var kitchenProductsArray = new JArray();
            foreach (var id in model.Command.IngredientIds)
            {
                var ingredientEntity = _repository.CalledIngredients.Set.FirstOrDefault(ci => ci.Id == id);
                if (ingredientEntity == null)
                {
                    var systemResponse = "Could not find ingredient by ID: " + id;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""get_recipe_ingredients"" }");
                }
                var kitchenProductLinkObject = new JObject();
                kitchenProductLinkObject["IngredientId"] = ingredientEntity.Id;
                kitchenProductLinkObject["KitchenProductId"] = ingredientEntity.KitchenProduct?.Id;
                kitchenProductLinkObject["Quantity"] = ingredientEntity.KitchenProduct?.Amount;
                kitchenProductLinkObject["KitchenUnitType"] = ingredientEntity.KitchenUnitType.ToString();
                kitchenProductsArray.Add(kitchenProductLinkObject);
            }

            model.Response.NavigateToPage = "recipes";
            return JsonConvert.SerializeObject(kitchenProductsArray, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}