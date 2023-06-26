using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_for_recipes_with_kitchen_products" })]
    public class ConsumeChatCommandSearchForRecipesWithKitchenProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchForRecipesWithKitchenProducts>
    {
        public ChatAICommandDTOSearchForRecipesWithKitchenProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchForRecipesWithKitchenProductsHandler : IRequestHandler<ConsumeChatCommandSearchForRecipesWithKitchenProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSearchForRecipesWithKitchenProductsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchForRecipesWithKitchenProducts model, CancellationToken cancellationToken)
        {
            var ids = model.Command.KitchenProducts.Select(ps => ps.KitchenProductId);
            var recipes = _repository.Recipes.Set.Where(r => r.CalledIngredients.Any(ci => ci.KitchenProduct != null && ids.Contains(ci.KitchenProduct.Id)));
            var results = new JArray();
            foreach (var recipe in recipes)
            {
                var recipeObject = new JObject();
                recipeObject["RecipeId"] = recipe.Id;
                recipeObject["RecipeName"] = recipe.Name;
                recipeObject["Serves"] = recipe.Serves;
                var recipeIngredientsArray = new JArray();
                foreach (var ingredient in recipe.CalledIngredients)
                {
                    var ingredientObject = new JObject();
                    ingredientObject["IngredientId"] = ingredient.Id;
                    ingredientObject["IngredientName"] = ingredient.Name;
                    ingredientObject["IngredientAmount"] = ingredient.Amount;
                    ingredientObject["IngredientKitchenUnitType"] = ingredient.KitchenUnitType.ToString();
                    recipeIngredientsArray.Add(ingredientObject);
                }
                recipeObject["Ingredients"] = recipeIngredientsArray;
                results.Add(recipeObject);
            }
            if (results.Count == 0)
            {
                return $"There are no recipes containing ingredients that are linked to any of the kitchen products.";
            }
            model.Response.NavigateToPage = "recipes";
            return JsonConvert.SerializeObject(results, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}