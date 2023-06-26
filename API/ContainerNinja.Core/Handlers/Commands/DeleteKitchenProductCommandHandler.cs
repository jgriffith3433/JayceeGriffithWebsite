using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Exceptions;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteKitchenProductCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteKitchenProductCommandHandler : IRequestHandler<DeleteKitchenProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteKitchenProductCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteKitchenProductCommand request, CancellationToken cancellationToken)
        {
            var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (kitchenProductEntity == null)
            {
                throw new NotFoundException($"No KitchenProduct found for the Id {request.Id}");
            }

            _cache.RemoveItem("product_stocks");
            _cache.RemoveItem($"product_stock_{request.Id}");
            _repository.KitchenProducts.Delete(request.Id);

            await _repository.CommitAsync();

            return request.Id;
        }
    }
}