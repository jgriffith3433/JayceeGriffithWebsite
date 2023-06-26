using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "link_recipe_ingredient_to_kitchen_product" })]
    public class ConsumeChatCommandLinkRecipeIngredientToKitchenProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOLinkRecipeIngredientToKitchenProduct>
    {
        public ChatAICommandDTOLinkRecipeIngredientToKitchenProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandLinkRecipeIngredientToKitchenProductHandler : IRequestHandler<ConsumeChatCommandLinkRecipeIngredientToKitchenProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandLinkRecipeIngredientToKitchenProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandLinkRecipeIngredientToKitchenProduct model, CancellationToken cancellationToken)
        {
            var recipeEntity = _repository.Recipes.Set.FirstOrDefault(r => r.Id == model.Command.RecipeId);
            if (recipeEntity == null)
            {
                var systemResponse = "Could not find recipe by ID: " + model.Command.RecipeId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""search_recipes"" }");
            }

            var ingredientEntity = recipeEntity.CalledIngredients.FirstOrDefault(i => i.Id == model.Command.IngredientId);
            if (ingredientEntity == null)
            {
                var systemResponse = "Could not find ingredient by ID: " + model.Command.IngredientId;
                throw new ChatAIException(systemResponse);
            }

            var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Id == model.Command.KitchenProductId);
            if (kitchenProductEntity == null)
            {
                var systemResponse = "Could not find kitchen product by ID: " + model.Command.KitchenProductId;
                throw new ChatAIException(systemResponse, @"{ ""name"": ""search_kitchen_products"" }");
            }

            ingredientEntity.KitchenProduct = kitchenProductEntity;
            _repository.CalledIngredients.Update(ingredientEntity);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "recipes";
            return $"Successfully linked IngredientId: {model.Command.IngredientId} to KitchenProductId: {model.Command.KitchenProductId}";
        }
    }
}