using MediatR;
using ContainerNinja.Contracts.Data;
using ContainerNinja.Contracts.DTO.ChatAICommands;
using ContainerNinja.Contracts.ViewModels;
using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Exceptions;
using ContainerNinja.Core.Common;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Handlers.ChatCommands
{
    [ChatCommandModel(new [] { "edit_product_unit_type" })]
    public class ConsumeChatCommandEditWalmartProductKitchenUnitType : IRequest<string>, IChatCommandConsumer<ChatAICommandDTOEditProductKitchenUnitType>
    {
        public ChatAICommandDTOEditProductKitchenUnitType Command { get; set; }
        public ChatResponseVM Response { get; set; }
    }

    public class ConsumeChatCommandEditProductKitchenUnitTypeHandler : IRequestHandler<ConsumeChatCommandEditWalmartProductKitchenUnitType, string>
    {
        private readonly IUnitOfWork _repository;

        public ConsumeChatCommandEditProductKitchenUnitTypeHandler(IUnitOfWork repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(ConsumeChatCommandEditWalmartProductKitchenUnitType model, CancellationToken cancellationToken)
        {
            var walmartProduct = _repository.WalmartProducts.Set.FirstOrDefault(p => p.Id == model.Command.ProductId);
            if (walmartProduct == null)
            {
                var systemResponse = "Could not find product by ID: " + model.Command.ProductId;
                throw new ChatAIException(systemResponse);
            }
            else
            {
                walmartProduct.KitchenUnitType = model.Command.KitchenUnitType;
                _repository.WalmartProducts.Update(walmartProduct);
            }
            model.Response.Dirty = _repository.ChangeTracker.HasChanges();
            model.Response.NavigateToPage = "walmart-products";
            return "Success";
        }
    }
}