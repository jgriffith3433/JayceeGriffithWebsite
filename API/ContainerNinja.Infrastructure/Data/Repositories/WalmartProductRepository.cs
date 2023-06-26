using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;
using Microsoft.EntityFrameworkCore;

namespace ContainerNinja.Core.Data.Repositories
{
    public class WalmartProductRepository : Repository<WalmartProduct>, IWalmartProductRepository
    {
        public WalmartProductRepository(DatabaseContext context) : base(context)
        {
        }
    }
}