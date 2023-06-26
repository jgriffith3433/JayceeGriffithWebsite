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
    [ChatCommandModel(new [] { "edit_consumed_recipe_ingredient" })]
    public class ConsumeChatCommandEditCookedRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditCookedRecipeIngredient>
    {
        public ChatAICommandDTOEditCookedRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteCookedRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandEditCookedRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteCookedRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditCookedRecipeIngredient model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.FirstOrDefault(cr => cr.Id == model.Command.LoggedRecipeId);
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find cooked recipe by ID: " + model.Command.LoggedRecipeId;
                throw new ChatAIException(systemResponse);
            }
            var cookedRecipeCalledIngredient = cookedRecipe.CookedRecipeCalledIngredients.FirstOrDefault(ci => ci.Id == model.Command.LoggedIngredientId);

            if (cookedRecipeCalledIngredient == null)
            {
                var systemResponse = "Could not find ingredient by ID: " + model.Command.LoggedIngredientId;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                cookedRecipeCalledIngredient.Name = model.Command.NewIngredientName;
                cookedRecipeCalledIngredient.CalledIngredient = null;
                cookedRecipeCalledIngredient.KitchenProduct = null;
                _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
            }
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
            return $"Substituted ingredient: {cookedRecipeCalledIngredient.Name} for {model.Command.NewIngredientName}\n" + JsonConvert.SerializeObject(cookedRecipeObject);
        }
    }
}