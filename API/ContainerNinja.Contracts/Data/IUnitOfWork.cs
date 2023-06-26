using ContainerNinja.Contracts.Data.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ContainerNinja.Contracts.Data
{
    public interface IUnitOfWork
    {
        ChangeTracker ChangeTracker { get; }
        IItemRepository Items { get; }
        ITodoListRepository TodoLists { get; }
        ITodoItemRepository TodoItems { get; }
        IUserRepository Users { get; }
        IChatCommandRepository ChatCommands { get; }
        IChatConversationRepository ChatConversations { get; }
        IWalmartProductRepository WalmartProducts { get; }
        ICompletedOrderRepository CompletedOrders { get; }
        ICompletedOrderProductRepository CompletedOrderProducts { get; }
        IKitchenProductRepository KitchenProducts { get; }
        ICalledIngredientRepository CalledIngredients { get; }
        IRecipeRepository Recipes { get; }
        ICookedRecipeRepository CookedRecipes { get; }
        ICookedRecipeCalledIngredientRepository CookedRecipeCalledIngredients { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        Task CommitAsync();
    }
}