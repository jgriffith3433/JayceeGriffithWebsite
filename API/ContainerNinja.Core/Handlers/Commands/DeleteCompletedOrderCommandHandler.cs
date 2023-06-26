using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteCompletedOrderCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteCompletedOrderCommandHandler : IRequestHandler<DeleteCompletedOrderCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteCompletedOrderCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteCompletedOrderCommand request, CancellationToken cancellationToken)
        {
            var completedOrderEntity = _repository.CompletedOrders.Set.FirstOrDefault(p => p.Id == request.Id);

            if (completedOrderEntity == null)
            {
                throw new NotFoundException($"No Completed Order found for the Id {request.Id}");
            }

            foreach(var completedOrderProduct in completedOrderEntity.CompletedOrderProducts)
            {
                _cache.RemoveItem($"completed_order_product_{completedOrderProduct.Id}");
                _repository.CompletedOrderProducts.Delete(completedOrderProduct.Id);
            }

            _cache.RemoveItem("completed_order_products");
            _repository.CompletedOrders.Delete(request.Id);

            await _repository.CommitAsync();

            _cache.RemoveItem("completed_orders");
            _cache.RemoveItem($"completed_order_{request.Id}");
            return completedOrderEntity.Id;
        }
    }
}