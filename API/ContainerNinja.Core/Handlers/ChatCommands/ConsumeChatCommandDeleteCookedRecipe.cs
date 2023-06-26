using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "delete_consumed_recipe" })]
    public class ConsumeChatCommandDeleteCookedRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteCookedRecipe>
    {
        public ChatAICommandDTODeleteCookedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteCookedRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteCookedRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteCookedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteCookedRecipe model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.FirstOrDefault(cr => cr.Id == model.Command.LoggedRecipeId);

            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find consumed recipe by ID: " + model.Command.LoggedRecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_consumed_recipe_id"" }");
            }
            foreach (var cookedRecipeCalledIngredient in cookedRecipe.CookedRecipeCalledIngredients)
            {
                _repository.CookedRecipeCalledIngredients.Delete(cookedRecipeCalledIngredient.Id);
            }
            _repository.CookedRecipes.Delete(cookedRecipe.Id);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            var cookedRecipeObject = new JObject();
            cookedRecipeObject["LoggedRecipeId"] = cookedRecipe.Id;
            if (cookedRecipe.Recipe != null)
            {
                cookedRecipeObject["RecipeName"] = cookedRecipe.Recipe.Name;
                cookedRecipeObject["Serves"] = cookedRecipe.Recipe.Serves;
            }
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in cookedRecipe.CookedRecipeCalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["LoggedIngredientId"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["IngredientAmount"] = ingredient.Amount;
                ingredientObject["IngredientKitchenUnitType"] = ingredient.KitchenUnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            cookedRecipeObject["Ingredients"] = recipeIngredientsArray;

            model.Response.NavigateToPage = "logged-recipes";
            return "Deleted log:\n" + JsonConvert.SerializeObject(cookedRecipeObject);
        }
    }
}