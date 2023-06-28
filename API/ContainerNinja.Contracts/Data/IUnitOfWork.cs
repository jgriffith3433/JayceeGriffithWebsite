using ContainerNinja.Contracts.Data.Repositories;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ContainerNinja.Contracts.Data
{
    public interface IUnitOfWork
    {
        ChangeTracker ChangeTracker { get; }
        IUserRepository Users { get; }
        IChatCommandRepository ChatCommands { get; }
        IChatConversationRepository ChatConversations { get; }
        Task CommitAsync();
    }
}