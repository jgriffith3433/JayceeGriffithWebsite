using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class DeleteWalmartProductCommand : IRequest<int>
    {
        public int Id { get; set; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteWalmartProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly ICachingService _cache;

        public DeleteProductCommandHandler(IUnitOfWork repository, ICachingService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<int> Handle(DeleteWalmartProductCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new NotFoundException($"No Product found for the Id {request.Id}");
            }

            _cache.RemoveItem("products");
            _cache.RemoveItem($"product_{request.Id}");
            _repository.WalmartProducts.Delete(request.Id);

            await _repository.CommitAsync();

            return productEntity.Id;
        }
    }
}