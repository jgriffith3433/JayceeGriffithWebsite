using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class CompletedOrderRepository : Repository<CompletedOrder>, ICompletedOrderRepository
    {
        public CompletedOrderRepository(DatabaseContext context) : base(context)
        {
        }
    }
}