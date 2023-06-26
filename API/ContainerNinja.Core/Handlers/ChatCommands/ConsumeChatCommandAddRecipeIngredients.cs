using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "create_new_recipe_ingredients" })]
    public class ConsumeChatCommandUpdateRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateRecipe>
    {
        public ChatAICommandDTOUpdateRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddRecipeIngredientsHandler : IRequestHandler<ConsumeChatCommandUpdateRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddRecipeIngredientsHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUpdateRecipe model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse);
            }

            foreach (var addRecipeIngredient in model.Command.KitchenProducts)
            {
                var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(r => r.Id == addRecipeIngredient.KitchenProductId);
                if (kitchenProductEntity == null)
                {
                    var systemResponse = "Could not find kitchen product by ID: " + addRecipeIngredient.KitchenProductId;
                    throw new ChatAIException(systemResponse);
                }

                var calledIngredientEntity = _repository.CalledIngredients.CreateProxy();
                {
                    calledIngredientEntity.Name = kitchenProductEntity.Name;
                    calledIngredientEntity.Amount = addRecipeIngredient.Quantity;
                    if (addRecipeIngredient.UnitType.HasValue)
                    {
                        calledIngredientEntity.KitchenUnitType = addRecipeIngredient.UnitType.Value;
                    }
                    else
                    {
                        calledIngredientEntity.KitchenUnitType = KitchenUnitType.Count;
                    }
                    calledIngredientEntity.KitchenProduct = kitchenProductEntity;
                };
                recipeEntity.CalledIngredients.Add(calledIngredientEntity);
            }
            _repository.Recipes.Update(recipeEntity);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            if (recipeEntity != null)
            {
                recipeObject["RecipeName"] = recipeEntity.Name;
                recipeObject["Serves"] = recipeEntity.Serves;
            }
            var recipeIngredientsArray = new JArray();
            foreach (var ingredient in recipeEntity.CalledIngredients)
            {
                var ingredientObject = new JObject();
                ingredientObject["IngredientId"] = ingredient.Id;
                ingredientObject["IngredientName"] = ingredient.Name;
                ingredientObject["KitchenProductId"] = ingredient.KitchenProduct?.Id;
                ingredientObject["IngredientAmount"] = ingredient.Amount;
                ingredientObject["IngredientKitchenUnitType"] = ingredient.KitchenUnitType.ToString();
                recipeIngredientsArray.Add(ingredientObject);
            }
            recipeObject["Ingredients"] = recipeIngredientsArray;
            model.Response.NavigateToPage = "recipes";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(recipeObject);
        }
    }
}