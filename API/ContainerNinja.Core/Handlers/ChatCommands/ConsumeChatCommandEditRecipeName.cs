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
    [ChatCommandModel(new[] { "edit_recipe_name_and_servings" })]
    public class ConsumeChatCommandEditRecipeName : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditRecipeName>
    {
        public ChatAICommandDTOEditRecipeName Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditRecipeNameHandler : IRequestHandler<ConsumeChatCommandEditRecipeName, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditRecipeNameHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditRecipeName model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);

            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""search_recipes"" }");
            }

            var existingRecipeWithName = _repository.Recipes.Set.FirstOrDefault(r => r.Name.ToLower() == model.Command.NewName.ToLower());
            if (existingRecipeWithName != null)
            {
                var systemResponse = "Recipe already exists: " + model.Command.NewName;
                throw new ChatAIException(systemResponse);
            }

            recipeEntity.Name = model.Command.NewName;
            recipeEntity.Serves = model.Command.Servings;
            _repository.Recipes.Update(recipeEntity);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            var recipeObject = new JObject();
            recipeObject["RecipeId"] = recipeEntity.Id;
            recipeObject["RecipeName"] = recipeEntity.Name;
            recipeObject["RecipeServings"] = recipeEntity.Serves;
            //var recipeIngredientsArray = new JArray();
            //foreach (var ingredient in recipeEntity.CalledIngredients)
            //{
            //    var ingredientObject = new JObject();
            //    ingredientObject["IngredientId"] = ingredient.Id;
            //    ingredientObject["IngredientName"] = ingredient.Name;
            //    recipeIngredientsArray.Add(ingredientObject);
            //}
            //recipeObject["Ingredients"] = recipeIngredientsArray;
            model.Response.NavigateToPage = "recipes";
            model.Response.ForceFunctionCall = "none";
            return JsonConvert.SerializeObject(recipeObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}