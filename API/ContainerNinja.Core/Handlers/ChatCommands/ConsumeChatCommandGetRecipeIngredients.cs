using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using ContainerNinja.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "get_recipe_ingredients" })]
    public class ConsumeChatCommandGetRecipeIngredients : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetRecipeIngredients>
    {
        public ChatAICommandDTOGetRecipeIngredients Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetRecipeIngredientsHandler : IRequestHandler<ConsumeChatCommandGetRecipeIngredients, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetRecipeIngredientsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetRecipeIngredients model, CancellationToken cancellationToken)
        {
            var recipe = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);

            if (recipe == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""search_recipes"" }");
            }

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipe.Id;
            recipeObject["RecipeName"] = recipe.Name;
            recipeObject["Servings"] = recipe.Serves;
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
            model.Response.NavigateToPage = "recipes";
            return JsonConvert.SerializeObject(recipeObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}