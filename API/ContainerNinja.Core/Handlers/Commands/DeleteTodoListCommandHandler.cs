using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteTodoListCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteTodoListCommandHandler : IRequestHandler<DeleteTodoListCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteTodoListCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteTodoListCommand request, CancellationToken cancellationToken)
        {
            _repository.TodoLists.Delete(request.Id);
            await _repository.CommitAsync();
            _cache.RemoveItem("todo_lists");
            _cache.RemoveItem($"todo_list_{request.Id}");
            return request.Id;
        }
    }
}