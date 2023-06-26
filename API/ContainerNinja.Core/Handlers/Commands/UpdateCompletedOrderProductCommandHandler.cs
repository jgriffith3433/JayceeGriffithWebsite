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
    public record UpdateCompletedOrderProductCommand : IRequest<CompletedOrderProductDTO>
    {
        public int Id { get; init; }

        public string? Name { get; init; }

        public long? WalmartId { get; init; }
    }

    public class UpdateCompletedOrderProductCommandHandler : IRequestHandler<UpdateCompletedOrderProductCommand, CompletedOrderProductDTO>
    {
        private readonly IUnitOfWork _repository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly ILogger<UpdateCompletedOrderProductCommandHandler> _logger;
        private readonly IWalmartService _walmartService;

        public UpdateCompletedOrderProductCommandHandler(ILogger<UpdateCompletedOrderProductCommandHandler> logger, IUnitOfWork repository, IMapper mapper, ICachingService cache, IWalmartService walmartService)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _walmartService = walmartService;
        }

        async Task<CompletedOrderProductDTO> IRequestHandler<UpdateCompletedOrderProductCommand, CompletedOrderProductDTO>.Handle(UpdateCompletedOrderProductCommand request, CancellationToken cancellationToken)
        {
            var completedOrderProductEntity = _repository.CompletedOrderProducts.Set.FirstOrDefault(cop => cop.Id == request.Id);

            if (completedOrderProductEntity == null)
            {
                throw new NotFoundException($"No Completed Order Product found for the Id {request.Id}");
            }

            if (completedOrderProductEntity.Name != request.Name && completedOrderProductEntity.WalmartId == null)
            {
                //Still searching for product
                completedOrderProductEntity.Name = request.Name;
                var searchResponse = await _walmartService.Search(completedOrderProductEntity.Name);
                completedOrderProductEntity.WalmartSearchResponse = JsonSerializer.Serialize(searchResponse);
            }
            //TODO: Look into this absurd if statement
            if ((completedOrderProductEntity.WalmartId == null && request.WalmartId != null) || (completedOrderProductEntity.WalmartId != null && completedOrderProductEntity.WalmartId != request.WalmartId || completedOrderProductEntity.WalmartProduct == null))
            {
                //Found walmart id by searching and user manually set the product by selecting walmart id
                completedOrderProductEntity.WalmartId = request.WalmartId;
                if (completedOrderProductEntity.WalmartId != null)
                {
                    try
                    {
                        var itemResponse = await _walmartService.GetItem(completedOrderProductEntity.WalmartId);
                        var serializedItemResponse = JsonSerializer.Serialize(itemResponse);
                        completedOrderProductEntity.WalmartItemResponse = serializedItemResponse;
                        completedOrderProductEntity.Name = itemResponse.name;

                        //TODO: needs to be look at after changing product-kitchenproducts relationship
                        //var productEntity = _repository.Products.Set.FirstOrDefault(p => p.WalmartId == itemResponse.itemId);

                        //if (productEntity != null)
                        //{
                        //    foreach (var kitchenProductEntity in productEntity.KitchenProducts)
                        //    {
                        //        kitchenProductEntity.Amount += 1;
                        //        _repository.KitchenProducts.Update(kitchenProductEntity);
                        //    }
                        //}
                        //else
                        //{
                        //    productEntity = _repository.Products.CreateProxy();
                        //    {
                        //        productEntity.Name = itemResponse.name;
                        //        productEntity.WalmartId = itemResponse.itemId;
                        //    };

                        //    //always ensure a kitchen product record exists for each product
                        //    productEntity.KitchenProducts = _repository.KitchenProducts.CreateProxy();
                        //    {
                        //        productEntity.KitchenProducts.Name = itemResponse.name;
                        //        productEntity.KitchenProducts.Amount = 1;
                        //        productEntity.KitchenProducts.WalmartProduct = productEntity;
                        //    };
                        //    _repository.KitchenProducts.Add(productEntity.KitchenProducts);
                        //    _repository.Products.Add(productEntity);
                        //}

                        //await _repository.CommitAsync();
                        //var walmartProductDTO = _mapper.Map<WalmartProductDTO>(productEntity);
                        //_cache.SetItem($"product_{productEntity.Id}", walmartProductDTO);
                        //_cache.RemoveItem("products");

                        //var kitchenProductDTO = _mapper.Map<KitchenProductDTO>(productEntity.KitchenProducts);
                        //_cache.SetItem($"product_stock_{productEntity.KitchenProducts.Id}", kitchenProductDTO);
                        //_cache.RemoveItem("product_stocks");

                        ////always update values from walmart to keep synced
                        //productEntity.WalmartItemResponse = serializedItemResponse;
                        //productEntity.Name = itemResponse.name;
                        //productEntity.Price = itemResponse.salePrice;
                        //productEntity.WalmartSize = itemResponse.size;
                        //productEntity.WalmartLink = string.Format("https://walmart.com/ip/{0}/{1}", itemResponse.name, itemResponse.itemId);
                        //completedOrderProductEntity.WalmartProduct = productEntity;
                        _repository.CompletedOrderProducts.Update(completedOrderProductEntity);
                    }
                    catch (Exception ex)
                    {
                        completedOrderProductEntity.WalmartError = ex.Message;
                    }
                }
            }
            await _repository.CommitAsync();

            var completedOrderProductDTO = _mapper.Map<CompletedOrderProductDTO>(completedOrderProductEntity);
            _cache.SetItem($"completed_order_product_{request.Id}", completedOrderProductDTO);
            _cache.RemoveItem("completed_orders");
            _cache.RemoveItem("completed_order_products");

            return completedOrderProductDTO;
        }
    }
}