using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Contracts.Data.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_consumed_recipes", "get_consumed_recipe"})]
    public class ConsumeChatCommandSearchCookedRecipes : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchCookedRecipes>
    {
        public ChatAICommandDTOSearchCookedRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchCookedRecipesHandler : IRequestHandler<ConsumeChatCommandSearchCookedRecipes, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSearchCookedRecipesHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchCookedRecipes model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<CookedRecipe>();
            var searchTerms = string.Join(' ', model.Command.Search.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Recipe.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.CookedRecipes.Set.AsExpandable().Where(predicate).ToList();
            var results = new JArray();
            foreach (var cookedRecipe in query)
            {
                var loggedRecipeObject = new JObject();
                loggedRecipeObject["LoggedRecipeId"] = cookedRecipe.Id;
                if (cookedRecipe.Recipe != null)
                {
                    loggedRecipeObject["RecipeName"] = cookedRecipe.Recipe.Name;
                    loggedRecipeObject["Serves"] = cookedRecipe.Recipe.Serves;
                }
                var recipeIngredientsArray = new JArray();
                foreach (var ingredient in cookedRecipe.CookedRecipeCalledIngredients)
                {
                    var ingredientObject = new JObject();
                    ingredientObject["LoggedIngredientId"] = ingredient.Id;
                    ingredientObject["IngredientName"] = ingredient.Name;
                    recipeIngredientsArray.Add(ingredientObject);
                }
                loggedRecipeObject["Ingredients"] = recipeIngredientsArray;
                results.Add(loggedRecipeObject);
            }
            model.Response.NavigateToPage = "logged-recipes";
            return JsonConvert.SerializeObject(results, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}