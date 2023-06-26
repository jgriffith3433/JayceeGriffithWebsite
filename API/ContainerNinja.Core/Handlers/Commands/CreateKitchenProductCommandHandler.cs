using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateKitchenProductCommand : IRequest<int>
    {
        public string Name { get; init; }
    }

    public class CreateKitchenProductCommandHandler : IRequestHandler<CreateKitchenProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateKitchenProductCommandHandler> _logger;
        private readonly ICachingService _cache;

        public CreateKitchenProductCommandHandler(ILogger<CreateKitchenProductCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<int> Handle(CreateKitchenProductCommand request, CancellationToken cancellationToken)
        {
            var existingKitchenProductWithName = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Name.ToLower() == request.Name.ToLower());

            if (existingKitchenProductWithName != null)
            {
                var systemResponse = "KitchenProduct already exists: " + request.Name;
                throw new Exception(systemResponse);
            }

            var kitchenProductEntity = _repository.KitchenProducts.CreateProxy();
            {
                kitchenProductEntity.Name = request.Name;
            };

            _repository.KitchenProducts.Add(kitchenProductEntity);

            await _repository.CommitAsync();

            var kitchenProductDTO = _mapper.Map<KitchenProductDTO>(kitchenProductEntity);
            _cache.SetItem($"product_stock_{kitchenProductDTO.Id}", kitchenProductDTO);
            _cache.RemoveItem("product_stocks");
            _cache.RemoveItem("products");
            return kitchenProductDTO.Id;
        }
    }
}