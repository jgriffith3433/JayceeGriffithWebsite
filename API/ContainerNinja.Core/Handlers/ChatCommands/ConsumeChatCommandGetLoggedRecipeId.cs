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
    [ChatCommandModel(new[] { "get_consumed_recipe_id" })]
    public class ConsumeChatCommandGetLoggedRecipeId : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetLoggedRecipeId>
    {
        public ChatAICommandDTOGetLoggedRecipeId Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandGetLoggedRecipeIdHandler : IRequestHandler<ConsumeChatCommandGetLoggedRecipeId, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandGetLoggedRecipeIdHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandGetLoggedRecipeId model, CancellationToken cancellationToken)
        {
            var predicate = PredicateBuilder.New<CookedRecipe>();
            var searchTerms = string.Join(' ', model.Command.LoggedRecipeName.ToLower().Split('-')).Split(' ');
            foreach (var searchTerm in searchTerms)
            {
                predicate = predicate.Or(p => p.Recipe.Name.ToLower().Contains(searchTerm));
            }

            var query = _repository.CookedRecipes.Set.AsExpandable().Where(predicate).ToList();

            CookedRecipe cookedRecipe;
            if (query.Count == 0)
            {
                var systemResponse = "Could not find consumed recipe by name: " + model.Command.LoggedRecipeName;
                throw new ChatAIException(systemResponse);
            }
            else if (query.Count == 1)
            {
                if (query[0].Recipe.Name.ToLower() == model.Command.LoggedRecipeName.ToLower())
                {
                    //exact match
                    cookedRecipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Could not find consumed recipe by name '" + model.Command.LoggedRecipeName + "'. Did you mean: '" + query[0].Recipe.Name + "' with ID: " + query[0].Id + "?";
                    throw new ChatAIException(systemResponse);
                }
            }
            else
            {
                var exactMatch = query.FirstOrDefault(r => r.Recipe.Name.ToLower() == model.Command.LoggedRecipeName.ToLower());
                if (exactMatch != null)
                {
                    //exact match
                    cookedRecipe = query[0];
                }
                else
                {
                    //unsure, ask user
                    var systemResponse = "Multiple logged records found: " + string.Join(", ", query.Select(r => r.Recipe.Name));
                    throw new ChatAIException(systemResponse);
                }
            }
            if (cookedRecipe == null)
            {
                var systemResponse = "Could not find consumed recipe by name '" + model.Command.LoggedRecipeName + "'.";
                throw new ChatAIException(systemResponse);
            }
            var loggedRecipeObject = new JObject();
            loggedRecipeObject["LoggedRecipeId"] = cookedRecipe.Id;
            model.Response.ForceFunctionCall = "auto";
            model.Response.NavigateToPage = "logged-recipes";
            return "LoggedRecipe:\n" + JsonConvert.SerializeObject(loggedRecipeObject);
        }
    }
}