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
    [ChatCommandModel(new[] { "delete_recipes" })]
    public class ConsumeChatCommandDeleteRecipes : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteRecipes>
    {
        public ChatAICommandDTODeleteRecipes Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeletesRecipeHandler : IRequestHandler<ConsumeChatCommandDeleteRecipes, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeletesRecipeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteRecipes model, CancellationToken cancellationToken)
        {
            var recipesDeletedArray = new JArray();
            foreach (var recipeIdToDelete in model.Command.RecipeIds)
            {
                var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == recipeIdToDelete);
                if (recipeEntity == null)
                {
                    var systemResponse = "Could not find recipe by ID: " + recipeIdToDelete;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_recipes"" }");
                }
                _repository.Recipes.Delete(recipeEntity.Id);

                var recipeObject = new JObject();
                recipeObject["RecipeId"] = recipeEntity.Id;
                recipeObject["RecipeName"] = recipeEntity.Name;
                recipesDeletedArray.Add(recipeObject);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();

            model.Response.NavigateToPage = "recipes";
            return JsonConvert.SerializeObject(recipesDeletedArray);
        }
    }
}