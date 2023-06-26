using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class CookedRecipeCalledIngredientRepository : Repository<CookedRecipeCalledIngredient>, ICookedRecipeCalledIngredientRepository
    {
        public CookedRecipeCalledIngredientRepository(DatabaseContext context) : base(context)
        {
        }
    }
}