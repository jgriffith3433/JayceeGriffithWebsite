using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Services;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductNameCommand : IRequest<WalmartProductDTO>
    {
        public int Id { get; init; }

        public string Name { get; init; }
    }

    public class UpdateProductNameCommandHandler : IRequestHandler<UpdateProductNameCommand, WalmartProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductNameCommandHandler> _logger;
        private readonly IWalmartService _walmartService;

        public UpdateProductNameCommandHandler(ILogger<UpdateProductNameCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _walmartService = walmartService;
        }

        async Task<WalmartProductDTO> IRequestHandler<UpdateProductNameCommand, WalmartProductDTO>.Handle(UpdateProductNameCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new NotFoundException($"No Product found for the Id {request.Id}");
            }

            if (productEntity.Name != request.Name && productEntity.WalmartId == null)
            {
                //Still searching for walmart product
                productEntity.Name = request.Name;

                var searchResponse = await _walmartService.Search(request.Name);
                productEntity.WalmartSearchResponse = JsonSerializer.Serialize(searchResponse);

                _repository.WalmartProducts.Update(productEntity);
                await _repository.CommitAsync();
            }

            _cache.RemoveItem("product_stocks");

            var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productEntity);
            _cache.SetItem($"product_{request.Id}", walmartProductDTO);
            _cache.RemoveItem("products");

            return walmartProductDTO;
        }
    }
}