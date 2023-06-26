using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Services;
using ContainerNinja.Contracts.Data.Entities;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "link_kitchen_products_to_walmart_products" })]
    public class ConsumeChatCommandLinkKitchenProductToWalmartProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOLinkKitchenProductsToWalmartProducts>
    {
        public ChatAICommandDTOLinkKitchenProductsToWalmartProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandLinkKitchenProductToWalmartProductHandler : IRequestHandler<ConsumeChatCommandLinkKitchenProductToWalmartProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandLinkKitchenProductToWalmartProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandLinkKitchenProductToWalmartProduct model, CancellationToken cancellationToken)
        {
            var notFoundWalmartIds = new List<long>();
            var kitchenProductsToUpdate = new List<KitchenProduct>();
            foreach (var link in model.Command.Links)
            {
                var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Id == link.KitchenProductId);
                if (kitchenProductEntity == null)
                {
                    var systemResponse = "Could not find kitchen product by ID: " + link.KitchenProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_kitchen_products"" }");
                }
                var walmartProductEntity = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == link.WalmartProductId);
                if (walmartProductEntity == null)
                {
                    var systemResponse = "Could not find walmart product by ID: " + link.WalmartProductId;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_walmart_products"" }");
                }
                kitchenProductEntity.WalmartProduct = walmartProductEntity;
                //var walmartItemResult = await _walmartService.GetItem(link.WalmartProductId);
                //if (walmartItemResult == null)
                //{
                //    notFoundWalmartIds.Add(link.WalmartProductId);
                //    continue;
                //}
                //kitchenProductEntity.Name = walmartItemResult.name;
                //kitchenProductEntity.WalmartProduct.WalmartId = link.WalmartProductId;
                ////always update values from walmart to keep synced
                //kitchenProductEntity.WalmartProduct.WalmartItemResponse = JsonConvert.SerializeObject(walmartItemResult);
                //kitchenProductEntity.WalmartProduct.Name = walmartItemResult.name;
                //kitchenProductEntity.WalmartProduct.Price = walmartItemResult.salePrice;
                //kitchenProductEntity.WalmartProduct.WalmartSize = walmartItemResult.size;
                //kitchenProductEntity.WalmartProduct.WalmartLink = string.Format("https://walmart.com/ip/{0}/{1}", walmartItemResult.name, walmartItemResult.itemId);
                kitchenProductsToUpdate.Add(kitchenProductEntity);
            }
            if (notFoundWalmartIds.Count > 0)
            {
                //var systemResponse = "Could not find the following walmart products by ID: " + string.Join(", ", notFoundWalmartIds);
                //throw new ChatAIException(systemResponse, @"{ ""name"": ""search_walmart_products_for_kitchen_product"" }");
            }

            foreach (var kitchenProductToUpdate in kitchenProductsToUpdate)
            {
                _repository.KitchenProducts.Update(kitchenProductToUpdate);
            }

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "kitchen-products";
            return $"Successfully linked {model.Command.Links.Count} walmart products";
        }
    }
}