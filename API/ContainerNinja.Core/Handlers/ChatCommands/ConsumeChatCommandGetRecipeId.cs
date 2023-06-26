//using MediatR;
//using ContainerNinja.Contracts.Data;
//using ContainerNinja.Contracts.DTO.ChatAICommands;
//using ContainerNinja.Contracts.ViewModels;
//using ContainerNinja.Core.Common;
//using ContainerNinja.Contracts.Data.Entities;
//using LinqKit;
//using ContainerNinja.Core.Exceptions;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace ContainerNinja.Core.Handlers.ChatCommands
//{
//    [ChatCommandModel(new[] { "get_recipe_id" })]
//    public class ConsumeChatCommandGetRecipeId : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOGetRecipeId>
//    {
//        public ChatAICommandDTOGetRecipeId Command { get; set; }
//        public ChatResponseVM Response { get; set; }
//    }

//    public class ConsumeChatCommandGetRecipeIdHandler : IRequestHandler<ConsumeChatCommandGetRecipeId, string>
//    {
//        private readonly IUnitOfWork _repository;

//        public ConsumeChatCommandGetRecipeIdHandler(IUnitOfWork repository)
//        {
//            _repository = repository;
//        }

//        public async Task<string> Handle(ConsumeChatCommandGetRecipeId model, CancellationToken cancellationToken)
//        {
//            var predicate = PredicateBuilder.New<Recipe>();
//            var searchTerms = string.Join(' ', model.Command.RecipeName.ToLower().Split('-')).Split(' ');
//            foreach (var searchTerm in searchTerms)
//            {
//                predicate = predicate.Or(p => p.Name.ToLower().Contains(searchTerm));
//            }

//            var query = _repository.Recipes.Set.AsExpandable().Where(predicate).ToList();

//            Recipe recipe;
//            if (query.Count == 0)
//            {
//                var systemResponse = "Could not find recipe by name: " + model.Command.RecipeName;
//                throw new ChatAIException(systemResponse);
//            }
//            else if (query.Count == 1)
//            {
//                if (query[0].Name.ToLower() == model.Command.RecipeName.ToLower())
//                {
//                    //exact match
//                    recipe = query[0];
//                }
//                else
//                {
//                    //unsure, ask user
//                    var systemResponse = "Could not find recipe by name '" + model.Command.RecipeName + "'. Did you mean: '" + query[0].Name + "' with ID: " + query[0].Id + "?";
//                    throw new ChatAIException(systemResponse);
//                }
//            }
//            else
//            {
//                var exactMatch = query.FirstOrDefault(r => r.Name.ToLower() == model.Command.RecipeName.ToLower());
//                if (exactMatch != null)
//                {
//                    //exact match
//                    recipe = query[0];
//                }
//                else
//                {
//                    //unsure, ask user
//                    var systemResponse = "Multiple records found: " + string.Join(", ", query.Select(r => r.Name));
//                    throw new ChatAIException(systemResponse);
//                }
//            }
//            if (recipe == null)
//            {
//                var systemResponse = "Could not find recipe by name '" + model.Command.RecipeName + "'.";
//                throw new ChatAIException(systemResponse);
//            }
//            var recipeObject = new JObject();
//            recipeObject["RecipeId"] = recipe.Id;
//            recipeObject["RecipeName"] = recipe.Name;
//            model.Response.ForceFunctionCall = "auto";
//            model.Response.NavigateToPage = "recipes";
//            return JsonConvert.SerializeObject(recipeObject);
//        }
//    }
//}