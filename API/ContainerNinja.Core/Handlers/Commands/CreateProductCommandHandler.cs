using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data.Entities;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Services;
using System.Text.Json;

namespace ContainerNinja.Core.Handlers.Commands
{
    public class CreateWalmartProductCommand : IRequest<int>
    {
        public string Name { get; init; }
    }

    public class CreateWalmartProductCommandHandler : IRequestHandler<CreateWalmartProductCommand, int>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateWalmartProductCommandHandler> _logger;
        private readonly ICachingService _cache;
        private readonly IWalmartService _walmartService;

        public CreateWalmartProductCommandHandler(ILogger<CreateWalmartProductCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _walmartService = walmartService;
        }

        public async Task<int> Handle(CreateWalmartProductCommand request, CancellationToken cancellationToken)
        {
            var existingProductWithName = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Name.ToLower() == request.Name.ToLower());

            if (existingProductWithName != null)
            {
                var systemResponse = "Walmart product already exists: " + request.Name;
                throw new Exception(systemResponse);
            }

            var productEntity = _repository.WalmartProducts.CreateProxy();
            {
                productEntity.Name = request.Name;
            };

            var searchResponse = await _walmartService.Search(request.Name);

            productEntity.WalmartSearchResponse = JsonSerializer.Serialize(searchResponse);

            _repository.WalmartProducts.Add(productEntity);

            await _repository.CommitAsync();

            _cache.RemoveItem("product_stocks");

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productEntity);
            _cache.SetItem($"product_{walmartProductDTO.Id}", walmartProductDTO);
            _cache.RemoveItem("products");
            return walmartProductDTO.Id;

        }
    }
}