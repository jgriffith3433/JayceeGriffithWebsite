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
    [ChatCommandModel(new[] { "search_recipes"})]
    public class ConsumeChatCommandSearchRecipes : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchRecipes>
    {
        public ChatAICommandDTOSearchRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchRecipesHandler : IRequestHandler<ConsumeChatCommandSearchRecipes, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandSearchRecipesHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchRecipes model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<Recipe>();
            if (string.IsNullOrEmpty(model.Command.Search))
            {
                predicate = predicate.Or(r => true);
            }
            else
            {
                var searchTerms = string.Join(' ', model.Command.Search.ToLower().Split('-')).Split(' ');
                foreach (var searchTerm in searchTerms)
                {
                    predicate = predicate.Or(r => r.Name.ToLower().Contains(searchTerm));
                }
            }
            var query = _repository.Recipes.Set.AsExpandable().Where(predicate).ToList();
            var results = new JArray();
            foreach (var recipe in query)
            {
                var recipeObject = new JObject();
                recipeObject["RecipeId"] = recipe.Id;
                recipeObject["RecipeName"] = recipe.Name;
                recipeObject["Servings"] = recipe.Serves;
                results.Add(recipeObject);
            }
            if (results.Count == 0)
            {
                return $"No Recipes matching the search term: {model.Command.Search}";
            }
            model.Response.NavigateToPage = "recipes";
            //model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(results, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}