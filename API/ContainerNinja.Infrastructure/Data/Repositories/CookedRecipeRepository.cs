using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class CookedRecipeRepository : Repository<CookedRecipe>, ICookedRecipeRepository
    {
        public CookedRecipeRepository(DatabaseContext context) : base(context)
        {
        }
    }
}