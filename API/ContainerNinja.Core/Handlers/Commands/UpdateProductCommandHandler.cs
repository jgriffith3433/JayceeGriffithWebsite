using MediatR;
using ContainerNinja.Contracts.DTO;
using ContainerNinja.Contracts.Data;
using FluentValidation;
using ContainerNinja.Core.Exceptions;
using AutoMapper;
using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Logging;
using ContainerNinja.Contracts.Data.Entities;
using ContainerNinja.Core.Services;
using System.Text.Json;

namespace ContainerNinja.Core.Handlers.Commands
{
    public record UpdateProductCommand : IRequest<WalmartProductDTO>
    {
        public int Id { get; init; }

        public long? WalmartId { get; init; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, WalmartProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateProductCommandHandler> _logger;
        private readonly IWalmartService _walmartService;

        public UpdateProductCommandHandler(ILogger<UpdateProductCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _walmartService = walmartService;
        }

        async Task<WalmartProductDTO> IRequestHandler<UpdateProductCommand, WalmartProductDTO>.Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var productEntity = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == request.Id);

            if (productEntity == null)
            {
                throw new NotFoundException($"No Product found for the Id {request.Id}");
            }

            if (productEntity.WalmartId != request.WalmartId && request.WalmartId != null)
            {
                //Found walmart id by searching and user manually set the product by selecting walmart id

                var existingProductEntity = _repository.WalmartProducts.GetAll().Where(p => p.WalmartId == request.WalmartId).FirstOrDefault();

                if (existingProductEntity != null)
                {
                    throw new NotFoundException($"Product already exists with that walmart id {request.WalmartId}");
                }

                productEntity.WalmartId = request.WalmartId;

                try
                {
                    var itemResponse = await _walmartService.GetItem(request.WalmartId);

                    var serializedItemResponse = JsonSerializer.Serialize(itemResponse);

                    //always update values from walmart to keep synced
                    productEntity.WalmartItemResponse = serializedItemResponse;
                    productEntity.Name = itemResponse.name;
                    productEntity.Price = itemResponse.salePrice;
                    productEntity.WalmartSize = itemResponse.size;
                    productEntity.WalmartLink = string.Format("https://walmart.com/ip/{0}/{1}", itemResponse.name, itemResponse.itemId);
                }
                catch (Exception ex)
                {
                    productEntity.Error = ex.Message;
                }
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