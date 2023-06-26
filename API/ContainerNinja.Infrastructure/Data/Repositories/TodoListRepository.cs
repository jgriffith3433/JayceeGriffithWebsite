using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Contracts.Data.Repositories;
using ContainerNinja.Migrations;

namespace ContainerNinja.Core.Data.Repositories
{
    public class TodoListRepository : Repository<TodoList>, ITodoListRepository
    {
        public TodoListRepository(DatabaseContext context) : base(context)
        {
        }
    }
}