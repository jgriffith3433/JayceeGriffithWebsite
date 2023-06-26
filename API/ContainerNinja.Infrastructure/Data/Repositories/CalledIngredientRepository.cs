using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class CalledIngredientRepository : Repository<CalledIngredient>, ICalledIngredientRepository
    {
        public CalledIngredientRepository(DatabaseContext context) : base(context)
        {
        }
    }
}