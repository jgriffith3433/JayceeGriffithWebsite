using Microsoft.EntityFrameworkCore;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace ContainerNinja.Core.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DatabaseContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DatabaseContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public DbSet<T> Set { get { return _dbSet; } }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public int Count()
        {
            return _dbSet.Count();
        }

        public void Delete(int id)
        {
            var entity = Get(id);
            if (entity != null)
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
                _dbSet.Remove(entity);
            }
        }

        public IIncludableQueryable<T, TProperty> Include<TEntity, TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath) where TEntity : class
        {
            return _dbSet.Include(navigationPropertyPath);
        }

        public T Get(int id)
        {
            var x = _dbSet.Find(id);
            return x;
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.AsEnumerable();
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public T CreateProxy()
        {
            return _dbSet.CreateProxy();
        }
    }
}