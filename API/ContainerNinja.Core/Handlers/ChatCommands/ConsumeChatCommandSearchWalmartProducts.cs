using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using ContainerNinja.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new[] { "search_walmart_products" })]
    public class ConsumeChatCommandSearchWalmartProducts : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOSearchWalmartProducts>
    {
        public ChatAICommandDTOSearchWalmartProducts Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandSearchWalmartProductsHandler : IRequestHandler<ConsumeChatCommandSearchWalmartProducts, string>
    {
        private readonly IUnitOfWork _repository;
        private readonly IWalmartService _walmartService;

        public ConsumeChatCommandSearchWalmartProductsHandler(IUnitOfWork repository, IWalmartService walmartService)
        {
            _repository = repository;
            _walmartService = walmartService;
        }

        public async Task<string> Handle(ConsumeChatCommandSearchWalmartProducts model, CancellationToken cancellationToken)
        {
            var walmartProductsArray = new JArray();
            var walmartSearchResults = await _walmartService.Search(model.Command.Name);
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
            kitchenProductWalmartProductsObject["WalmartSearchResults"] = walmartProductsArray;
            model.Response.ForceFunctionCall = "none";
            model.Response.NavigateToPage = "walmart-products";
            return JsonConvert.SerializeObject(kitchenProductWalmartProductsObject, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}