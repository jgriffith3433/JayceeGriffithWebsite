using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "update_consumed_recipe" })]
    public class ConsumeChatCommandUpdateLoggedRecipe : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOUpdateLoggedRecipe>
    {
        public ChatAICommandDTOUpdateLoggedRecipe Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandUpdateLoggedRecipeHandler : IRequestHandler<ConsumeChatCommandUpdateLoggedRecipe, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandUpdateLoggedRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandUpdateLoggedRecipe model, CancellationToken cancellationToken)
        {
            var cookedRecipe = _repository.CookedRecipes.Set.OrderByDescending(cr => cr.Created).FirstOrDefault(r => r.Recipe.Id == model.Command.LoggedRecipeId);
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find consumed recipe by ID: " + model.Command.LoggedRecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""get_consumed_recipe_id"" }");
            }

            foreach(var updateLoggedRecipeIngredient in model.Command.Ingredients)
            {
                var cookedRecipeCalledIngredient = _repository.CookedRecipeCalledIngredients.CreateProxy();
                {
                    cookedRecipeCalledIngredient.Name = updateLoggedRecipeIngredient.IngredientName;
                    //cookedRecipeCalledIngredient.CookedRecipe = cookedRecipe;
                    cookedRecipeCalledIngredient.Amount = updateLoggedRecipeIngredient.Quantity;
                    cookedRecipeCalledIngredient.KitchenUnitType = updateLoggedRecipeIngredient.KitchenUnitType;
                };
                cookedRecipe.CookedRecipeCalledIngredients.Add(cookedRecipeCalledIngredient);
            }
            
            _repository.CookedRecipes.Update(cookedRecipe);
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var cookedRecipeObject = new JObject();
            cookedRecipeObject["LoogedRecipeId"] = cookedRecipe.Id;
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
            return JsonConvert.SerializeObject(cookedRecipeObject);
        }
    }
}