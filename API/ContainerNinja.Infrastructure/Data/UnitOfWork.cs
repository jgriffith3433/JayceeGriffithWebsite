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

        public IUserRepository Users => new UserRepository(_context);
        public IChatCommandRepository ChatCommands => new ChatCommandRepository(_context);
        public IChatConversationRepository ChatConversations => new ChatConversationRepository(_context);

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}