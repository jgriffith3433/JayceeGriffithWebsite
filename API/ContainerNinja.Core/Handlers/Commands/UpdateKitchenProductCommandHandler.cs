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
    public record UpdateKitchenProductCommand : IRequest<KitchenProductDTO>
    {
        public int Id { get; init; }

        public float Amount { get; init; }
    }

    public class UpdateKitchenProductCommandHandler : IRequestHandler<UpdateKitchenProductCommand, KitchenProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateKitchenProductCommandHandler> _logger;

        public UpdateKitchenProductCommandHandler(ILogger<UpdateKitchenProductCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        async Task<KitchenProductDTO> IRequestHandler<UpdateKitchenProductCommand, KitchenProductDTO>.Handle(UpdateKitchenProductCommand request, CancellationToken cancellationToken)
        {
            var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(ps => ps.Id == request.Id);

            if (kitchenProductEntity == null)
            {
                throw new NotFoundException($"No KitchenProduct found for the Id {request.Id}");
            }

            kitchenProductEntity.Amount = request.Amount;

            _repository.KitchenProducts.Update(kitchenProductEntity);

            await _repository.CommitAsync();

            var kitchenProductDTO = _mapper.Map<KitchenProductDTO>(kitchenProductEntity);
            _cache.Clear();
            _cache.SetItem($"product_stock_{request.Id}", kitchenProductDTO);

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(kitchenProductEntity.WalmartProduct);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            return kitchenProductDTO;
        }
    }
}