using Microsoft.EntityFrameworkCore;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Migrations
{
    public class DatabaseContext : DbContext
    {
        private readonly IUserService _user;

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IUserService user) : base(options)
        {
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _user = user;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var item in ChangeTracker.Entries<User>().AsEnumerable())
            {
                if (item.State == EntityState.Added)
                {
                    item.Entity.Created = DateTime.UtcNow;
                }
            }

            foreach (var item in ChangeTracker.Entries<AuditableEntity>().AsEnumerable())
            {
                if (item.State == EntityState.Added)
                {
                    item.Entity.Created = DateTime.UtcNow;
                    item.Entity.CreatedBy = _user.UserId;
                }
                else if (item.State == EntityState.Modified)
                {
                    item.Entity.LastModified = DateTime.UtcNow;
                    item.Entity.ModifiedBy = _user.UserId;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatCommand> ChatCommands { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
    }
}