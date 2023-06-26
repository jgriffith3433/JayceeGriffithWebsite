using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteCompletedOrderProductCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteCompletedOrderProductCommandHandler : IRequestHandler<DeleteCompletedOrderProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteCompletedOrderProductCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteCompletedOrderProductCommand request, CancellationToken cancellationToken)
        {
            var completedOrderProductEntity = _repository.CompletedOrderProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (completedOrderProductEntity == null)
            {
                throw new NotFoundException($"No Completed Order Product found for the Id {request.Id}");
            }

            _cache.RemoveItem($"completed_order_product_{completedOrderProductEntity.Id}");
            _repository.CompletedOrderProducts.Delete(completedOrderProductEntity.Id);

            await _repository.CommitAsync();

            _cache.RemoveItem("completed_order_products");
            _cache.RemoveItem("completed_orders");
            _cache.RemoveItem($"completed_order_{request.Id}");
            return completedOrderProductEntity.Id;
        }
    }
}