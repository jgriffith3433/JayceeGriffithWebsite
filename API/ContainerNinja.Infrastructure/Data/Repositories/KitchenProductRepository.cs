using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class KitchenProductRepository : Repository<KitchenProduct>, IKitchenProductRepository
    {
        public KitchenProductRepository(DatabaseContext context) : base(context)
        {
        }
    }
}