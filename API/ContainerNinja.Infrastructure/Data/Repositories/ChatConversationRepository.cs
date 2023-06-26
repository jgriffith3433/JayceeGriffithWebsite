using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class ChatConversationRepository : Repository<ChatConversation>, IChatConversationRepository
    {
        public ChatConversationRepository(DatabaseContext context) : base(context)
        {
        }
    }
}