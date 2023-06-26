using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ContainerNinja.Contracts.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        DbSet<T> Set { get; }
        T Get(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        int Count();
        T CreateProxy();
    }
}