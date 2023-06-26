using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductKitchenUnitTypeCommand : IRequest<WalmartProductDTO>
    {
        public int Id { get; init; }

        public int KitchenUnitType { get; init; }
    }

    public class UpdateProductKitchenUnitTypeCommandHandler : IRequestHandler<UpdateProductKitchenUnitTypeCommand, WalmartProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductKitchenUnitTypeCommandHandler> _logger;

        public UpdateProductKitchenUnitTypeCommandHandler(ILogger<UpdateProductKitchenUnitTypeCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<WalmartProductDTO> IRequestHandler<UpdateProductKitchenUnitTypeCommand, WalmartProductDTO>.Handle(UpdateProductKitchenUnitTypeCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new NotFoundException($"No Product found for the Id {request.Id}");
            }

            productEntity.KitchenUnitType = (KitchenUnitType)request.KitchenUnitType;
            _repository.WalmartProducts.Update(productEntity);
            await _repository.CommitAsync();

            var kitchenProductDTO = _mapper.Map<KitchenProductDTO>(productEntity.KitchenProducts);
            _cache.SetItem($"product_stock_{request.Id}", kitchenProductDTO);
            _cache.RemoveItem("product_stocks");

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            _cache.RemoveItem("products");

            return walmartProductDTO;
        }
    }
}