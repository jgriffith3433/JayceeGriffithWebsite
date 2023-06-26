using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_recipe_ingredient" })]
    public class ConsumeChatCommandDeleteRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteRecipeIngredient>
    {
        public ChatAICommandDTODeleteRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandDeleteRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }

            var calledIngredient = recipeEntity.CalledIngredients.FirstOrDefault(ci => ci.Name.ToLower().Contains(model.Command.IngredientName.ToLower()));

            if (calledIngredient == null)
            {
                var systemResponse = "No ingredient '" + model.Command.IngredientName + "' found on recipe '" + model.Command.RecipeId + "'. The ingredients are: " + string.Join(", ", recipeEntity.CalledIngredients.Select(ci => ci.Name));
                throw new ChatAIException(systemResponse);
            }

            recipeEntity.CalledIngredients.Remove(calledIngredient);
            _repository.Recipes.Update(recipeEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            recipeObject["RecipeName"] = recipeEntity.Name;
            recipeObject["Serves"] = recipeEntity.Serves;
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipeEntity.CalledIngredients)
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
            return "Created recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}