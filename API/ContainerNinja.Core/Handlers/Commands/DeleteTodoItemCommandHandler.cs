using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteTodoItemCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteTodoItemCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
        {
            var todoItem = _repository.TodoItems.Set.FirstOrDefault(x => x.Id == request.Id);
            if (todoItem == null)
            {
                throw new NotFoundException($"No TodoItem found for the Id {request.Id}");
            }

            _repository.TodoItems.Delete(todoItem.Id);
            await _repository.CommitAsync();

            _cache.RemoveItem($"todo_list_{todoItem.ListId}");
            _cache.RemoveItem($"todo_lists");
            return request.Id;
        }
    }
}