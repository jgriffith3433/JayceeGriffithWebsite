using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteItemCommand : IRequest<int>
    {
        public int ItemId { get; }

        public DeleteItemCommand(int itemId)
        {
            ItemId = itemId;
        }
    }

    public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteItemCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
        {
            _repository.Items.Delete(request.ItemId);
            await _repository.CommitAsync();
            _cache.RemoveItem("items");
            _cache.RemoveItem($"item_{request.ItemId}");
            return request.ItemId;
        }
    }
}