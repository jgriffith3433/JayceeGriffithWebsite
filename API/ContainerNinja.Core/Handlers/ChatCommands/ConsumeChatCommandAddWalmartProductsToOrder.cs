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
    [ChatCommandModel("add_walmart_products_to_order")]
    public class ConsumeChatCommandAddWalmartProductsToOrder : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOAddWalmartProductsToOrder>
    {
        public ChatAICommandDTOAddWalmartProductsToOrder Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandAddWalmartProductsToOrderHandler : IRequestHandler<ConsumeChatCommandAddWalmartProductsToOrder, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandAddWalmartProductsToOrderHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandAddWalmartProductsToOrder model, CancellationToken cancellationToken)
        {
            var orderEntity = _repository.Orders.Set.FirstOrDefault(o => o.Id == model.Command.OrderId);
            if (orderEntity == null)
            {
                var systemResponse = "Could not find order by ID: " + model.Command.OrderId;
                throw new ChatAIException(systemResponse);//, @"{ ""name"": ""get_order_id"" }");
            }


            foreach (var walmartProductToOrder in model.Command.WalmartProducts)
            {
                var walmartProductEntity = _repository.WalmartProducts.Set.FirstOrDefault(ps => ps.Id == walmartProductToOrder.WalmartProductId);

                if (walmartProductEntity == null)
                {
                    var systemResponse = "Could not find walmart product by ID: " + walmartProductToOrder.WalmartProductId;
                    throw new ChatAIException(systemResponse);//, @"{ ""name"": ""get_order_id"" }");
                }

                var orderItemEntity = _repository.OrderItems.CreateProxy();
                {
                    orderItemEntity.WalmartProduct = walmartProductEntity;
                    orderItemEntity.WalmartId = walmartProductEntity.WalmartId;
                    orderItemEntity.Name = walmartProductEntity.Name;
                    //TODO: Convert from unit type to walmart size, for now just forcing conversion to satisfy demo
                    orderItemEntity.Quantity = (int)walmartProductToOrder.Quantity;
                }
                _repository.OrderItems.Add(orderItemEntity);
                orderEntity.OrderItems.Add(orderItemEntity);
            }
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