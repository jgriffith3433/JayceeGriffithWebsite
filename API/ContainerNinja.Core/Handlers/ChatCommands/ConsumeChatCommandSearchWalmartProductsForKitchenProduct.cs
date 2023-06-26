using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json.Linq;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Contracts.Data.Entities;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_walmart_products_for_kitchen_product" })]
    public class ConsumeChatCommandSearchWalmartProductsForKitchenProduct : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchWalmartProductsForKitchenProduct>
    {
        public ChatAICommandDTOSearchWalmartProductsForKitchenProduct Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchWalmartProductsForKitchenProductHandler : IRequestHandler<ConsumeChatCommandSearchWalmartProductsForKitchenProduct, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandSearchWalmartProductsForKitchenProductHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchWalmartProductsForKitchenProduct model, CancellationToken cancellationToken)
        {
            var kitchenProductsToFindWalmartProducts = new List<KitchenProduct>();
            var kitchenProductWalmartProducts = new JArray();
            foreach (var id in model.Command.KitchenProductIds)
            {
                var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(p => p.Id == id);
                if (kitchenProductEntity == null)
                {
                    var systemResponse = "Could not find kitchen product by ID: " + id;
                    throw new ChatAIException(systemResponse, @"{ ""name"": ""search_kitchen_products"" }");
                }
                kitchenProductsToFindWalmartProducts.Add(kitchenProductEntity);
            }

            foreach (var kitchenProductToFindWalmartProduct in kitchenProductsToFindWalmartProducts)
            {
                var walmartProductsArray = new JArray();
                var walmartSearchResults = await _walmartService.Search(kitchenProductToFindWalmartProduct.Name);
                if (walmartSearchResults != null && walmartSearchResults.items != null)
                {
                    foreach (var walmartItem in walmartSearchResults.items)
                    {
                        var walmartProductEntity = _repository.WalmartProducts.Set.FirstOrDefault(wp => wp.WalmartId == walmartItem.itemId);
                        if (walmartProductEntity == null)
                        {
                            walmartProductEntity = _repository.WalmartProducts.CreateProxy();
                            {
                                _repository.WalmartProducts.Add(walmartProductEntity);
                            }
                        }
                        else
                        {
                            _repository.WalmartProducts.Update(walmartProductEntity);
                        }
                        //always update values from walmart to keep synced
                        walmartProductEntity.Name = walmartItem.name;
                        walmartProductEntity.WalmartId = walmartItem.itemId;
                        walmartProductEntity.WalmartItemResponse = JsonConvert.SerializeObject(walmartItem);
                        walmartProductEntity.Name = walmartItem.name;
                        walmartProductEntity.Price = walmartItem.salePrice;
                        walmartProductEntity.WalmartSize = walmartItem.size;
                        walmartProductEntity.WalmartLink = string.Format("https://walmart.com/ip/{0}/{1}", walmartItem.name, walmartItem.itemId);
                        var walmartProductObject = new JObject();
                        walmartProductObject["WalmartProductId"] = walmartProductEntity.Id;
                        walmartProductObject["WalmartProductName"] = walmartProductEntity.Name;
                        //walmartProductObject["WalmartProductSize"] = walmartProductEntity.size;
                        walmartProductsArray.Add(walmartProductObject);
                    }
                }
                var kitchenProductWalmartProductsObject = new JObject();
                kitchenProductWalmartProductsObject["KitchenProductId"] = kitchenProductToFindWalmartProduct.Id;
                kitchenProductWalmartProductsObject["WalmartSearchResults"] = walmartProductsArray;
                kitchenProductWalmartProducts.Add(kitchenProductWalmartProductsObject);
            }
            model.Response.ForceFunctionCall = "none";
            model.Response.NavigateToPage = "walmart-products";
            return JsonConvert.SerializeObject(kitchenProductWalmartProducts, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}