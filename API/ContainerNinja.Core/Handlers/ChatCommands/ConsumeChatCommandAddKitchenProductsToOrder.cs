using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    //[ChatCommandModel("add_kitchen_products_to_order")]
    public class ConsumeChatCommandAddKitchenProductsToOrder : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddKitchenProductsToOrder>
    {
        public ChatAICommandDTOAddKitchenProductsToOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddKitchenProductsToOrderHandler : IRequestHandler<ConsumeChatCommandAddKitchenProductsToOrder, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddKitchenProductsToOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddKitchenProductsToOrder model, CancellationToken cancellationToken)
        {
            var orderEntity = _repository.Orders.Set.FirstOrDefault(o => o.Id == model.Command.OrderId);
            if (orderEntity == null)
            {
                var systemResponse = "Could not find order by ID: " + model.Command.OrderId;
                throw new ChatAIException(systemResponse);//, @"{ ""name"": ""get_order_id"" }");
            }

            var notLinkedToWalmartProducts = new List<int>();
            var noUnitConversionToWalmartProducts = new List<int>();
            //foreach (var kitchenProductToOrder in model.Command.KitchenProducts)
            //{
            //    var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(ps => ps.Id == kitchenProductToOrder.KitchenProductId);
            //    if (kitchenProductEntity == null)
            //    {
            //        var systemResponse = "Could not find kitchen product by ID: " + kitchenProductToOrder.KitchenProductId;
            //        throw new ChatAIException(systemResponse, @"{ ""name"": ""search_kitchen_products"" }");
            //    }

            //    if (kitchenProductEntity.WalmartProduct == null)
            //    {
            //        notLinkedToWalmartProducts.Add(kitchenProductToOrder.KitchenProductId);
            //        var systemResponse = $"Kitchen Product ({kitchenProductToOrder.KitchenProductId}) is not linked to a walmart product yet";
            //    }

            //    if (kitchenProductEntity.WalmartProduct.WalmartId.HasValue == false)
            //    {
            //        notLinkedToWalmartProducts.Add(kitchenProductToOrder.KitchenProductId);
            //        var systemResponse = $"Kitchen Product ({kitchenProductToOrder.KitchenProductId}) is not linked to a walmart product yet";
            //    }
            //}
            if (notLinkedToWalmartProducts.Count > 0)
            {
                var systemResponse = "The following kitchen products are not linked to walmart products: " + string.Join(", ", notLinkedToWalmartProducts);
                throw new ChatAIException(systemResponse, @"{ ""name"": ""link_kitchen_products_to_walmart_products"" }");
            }

            //foreach (var kitchenProductToOrder in model.Command.KitchenProducts)
            //{
            //    var kitchenProductEntity = _repository.KitchenProducts.Set.FirstOrDefault(ps => ps.Id == kitchenProductToOrder.KitchenProductId);

            //    var orderItemEntity = _repository.OrderItems.CreateProxy();
            //    {
            //        orderItemEntity.Product = kitchenProductEntity.WalmartProduct;
            //        orderItemEntity.WalmartId = kitchenProductEntity.WalmartProduct.WalmartId;
            //        orderItemEntity.Name = kitchenProductEntity.Name;
            //        orderItemEntity.Quantity = kitchenProductToOrder.OrderQuantity;
            //    }
            //    _repository.OrderItems.Add(orderItemEntity);
            //    orderEntity.OrderItems.Add(orderItemEntity);
            //}
            _repository.Orders.Update(orderEntity);

            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            await _repository.CommitAsync();

            var orderObject = new JObject();
            orderObject["OrderId"] = orderEntity.Id;
            var orderProductsArray = new JArray();
            foreach (var orderProduct in orderEntity.OrderItems)
            {
                var ingredientObject = new JObject();
                ingredientObject["OrderItemId"] = orderProduct.Id;
                ingredientObject["OrderItemName"] = orderProduct.Name;
                ingredientObject["OrderItemWalmartId"] = orderProduct.WalmartId;
                orderProductsArray.Add(ingredientObject);
            }
            orderObject["OrderItems"] = orderProductsArray;
            model.Response.NavigateToPage = "orders";
            return JsonConvert.SerializeObject(orderObject, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}