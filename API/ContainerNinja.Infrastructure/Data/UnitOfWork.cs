using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Core.Data.Repositories;
using ContainerNinja.Migrations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ContainerNinja.Core.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }

        public ChangeTracker ChangeTracker
        {
            get { return _context.ChangeTracker; }
        }

        public IItemRepository Items => new ItemRepository(_context);
        public ITodoListRepository TodoLists => new TodoListRepository(_context);
        public ITodoItemRepository TodoItems => new TodoItemRepository(_context);
        public IUserRepository Users => new UserRepository(_context);
        public IChatCommandRepository ChatCommands => new ChatCommandRepository(_context);
        public IChatConversationRepository ChatConversations => new ChatConversationRepository(_context);
        public IWalmartProductRepository WalmartProducts => new WalmartProductRepository(_context);
        public ICompletedOrderRepository CompletedOrders => new CompletedOrderRepository(_context);
        public ICompletedOrderProductRepository CompletedOrderProducts => new CompletedOrderProductRepository(_context);
        public IKitchenProductRepository KitchenProducts => new KitchenProductRepository(_context);
        public ICalledIngredientRepository CalledIngredients => new CalledIngredientRepository(_context);
        public IRecipeRepository Recipes => new RecipeRepository(_context);
        public ICookedRecipeRepository CookedRecipes => new CookedRecipeRepository(_context);
        public ICookedRecipeCalledIngredientRepository CookedRecipeCalledIngredients => new CookedRecipeCalledIngredientRepository(_context);
        public IOrderRepository Orders => new OrderRepository(_context);
        public IOrderItemRepository OrderItems => new OrderItemRepository(_context);

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}