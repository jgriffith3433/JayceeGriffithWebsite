using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "delete_kitchen_products" })]
    public class ConsumeChatCommandDeleteKitchenProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTODeleteKitchenProducts>
    {
        public ChatAICommandDTODeleteKitchenProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandDeleteKitchenProductHandler : IRequestHandler<ConsumeChatCommandDeleteKitchenProducts, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandDeleteKitchenProductHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandDeleteKitchenProducts model, CancellationToken cancellationToken)
        {
            foreach (var productToDelete in model.Command.KitchenProductsToDelete)
            {
                var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Id == productToDelete.KitchenProductId);
                if (kitchenProductEntity == null)
                {
                    var systemResponse = "Could not find kitchen product by ID: " + productToDelete.KitchenProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_kitchen_products"" }");
                }
                if (kitchenProductEntity != null)
                {
                    var calledIngredients = _repository.CalledIngredients.Set.Where(ci => ci.KitchenProduct != null && ci.KitchenProduct == kitchenProductEntity);
                    foreach (var calledIngredient in calledIngredients)
                    {
                        calledIngredient.KitchenProduct = null;
                        _repository.CalledIngredients.Update(calledIngredient);
                    }
                    var cookedRecipeCalledIngredients = _repository.CookedRecipeCalledIngredients.Set.Where(ci => ci.KitchenProduct != null && ci.KitchenProduct == kitchenProductEntity);
                    foreach (var cookedRecipeCalledIngredient in cookedRecipeCalledIngredients)
                    {
                        cookedRecipeCalledIngredient.KitchenProduct = null;
                        _repository.CookedRecipeCalledIngredients.Update(cookedRecipeCalledIngredient);
                    }

                    _repository.KitchenProducts.Delete(kitchenProductEntity.Id);
                }
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "kitchen-products";
            return "Success";
        }
    }
}