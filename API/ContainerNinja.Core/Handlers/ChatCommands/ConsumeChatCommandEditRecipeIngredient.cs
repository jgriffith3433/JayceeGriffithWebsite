using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    //[ChatCommandModel(new [] { "edit_recipe_ingredient" })]
    public class ConsumeChatCommandEditRecipeIngredient : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditRecipeIngredient>
    {
        public ChatAICommandDTOEditRecipeIngredient Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSubstituteRecipeIngredientHandler : IRequestHandler<ConsumeChatCommandEditRecipeIngredient, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSubstituteRecipeIngredientHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditRecipeIngredient model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }
            var calledIngredient = recipeEntity.CalledIngredients.FirstOrDefault(ci => ci.Id == model.Command.IngredientId);

            if (calledIngredient == null)
            {
                var systemResponse = "Could not find ingredient by ID: " + model.Command.IngredientId;
                throw new ChatAIException(systemResponse);
            }
            calledIngredient.Name = model.Command.NewIngredientName;
            calledIngredient.KitchenProduct = null;
            _repository.CalledIngredients.Update(calledIngredient);
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
            return "Recipe:\n" + JsonConvert.SerializeObject(recipeObject);
        }
    }
}