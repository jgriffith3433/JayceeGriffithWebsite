using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateKitchenProductDetailsCommand : IRequest<KitchenProductDetailsDTO>
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public int ProductId { get; init; }

        public float Amount { get; init; }
    }

    public class UpdateKitchenProductDetailsCommandHandler : IRequestHandler<UpdateKitchenProductDetailsCommand, KitchenProductDetailsDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateKitchenProductDetailsCommandHandler> _logger;

        public UpdateKitchenProductDetailsCommandHandler(ILogger<UpdateKitchenProductDetailsCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<KitchenProductDetailsDTO> IRequestHandler<UpdateKitchenProductDetailsCommand, KitchenProductDetailsDTO>.Handle(UpdateKitchenProductDetailsCommand request, CancellationToken cancellationToken)
        {
            var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(ps => ps.Id == request.Id);

            if (kitchenProductEntity == null)
            {
                throw new NotFoundException($"No KitchenProduct found for the Id {request.Id}");
            }

            if (kitchenProductEntity.WalmartProduct == null || request.ProductId != kitchenProductEntity.WalmartProduct.Id)
            {
                //First search for kitchen products that are already linked to the product
                var alreadyLinkedKitchenProduct = _repository.KitchenProducts.Set.Where(p => p.WalmartProduct.Id == request.ProductId).FirstOrDefault();
                if (alreadyLinkedKitchenProduct != null)
                {
                    //we found an existing kitchen product, merge
                    var calledIngredients = _repository.CalledIngredients.Set.Where(ci => ci.KitchenProduct != null && ci.KitchenProduct.Id == kitchenProductEntity.Id);
                    foreach (var calledIngredient in calledIngredients)
                    {
                        calledIngredient.KitchenProduct = alreadyLinkedKitchenProduct;
                        _repository.CalledIngredients.Update(calledIngredient);
                    }
                    _repository.KitchenProducts.Delete(kitchenProductEntity.Id);
                    kitchenProductEntity = alreadyLinkedKitchenProduct;
                }
                else
                {
                    //no kitchen product found that was already linked to product, link this one
                    var productEntity = _repository.WalmartProducts.Get(request.ProductId);
                    if (kitchenProductEntity == null)
                    {
                        throw new NotFoundException($"No KitchenProduct found for the Id {request.ProductId}");
                    }
                    kitchenProductEntity.WalmartProduct = productEntity;
                }
            }

            kitchenProductEntity.Amount = request.Amount;
            kitchenProductEntity.Name = request.Name;

            _repository.KitchenProducts.Update(kitchenProductEntity);

            await _repository.CommitAsync();

            var kitchenProductDTO = _mapper.Map<KitchenProductDTO>(kitchenProductEntity);
            _cache.SetItem($"product_stock_{request.Id}", kitchenProductDTO);
            _cache.RemoveItem("product_stocks");

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(kitchenProductEntity.WalmartProduct);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            _cache.RemoveItem("products");

            return _mapper.Map<KitchenProductDetailsDTO>(kitchenProductDTO);
        }
    }
}