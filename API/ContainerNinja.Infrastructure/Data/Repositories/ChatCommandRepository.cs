using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class ChatCommandRepository : Repository<ChatCommand>, IChatCommandRepository
    {
        public ChatCommandRepository(DatabaseContext context) : base(context)
        {
        }
    }
}