using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddKitchenProductsToOrderValidator : AbstractValidator<ConsumeChatCommandAddKitchenProductsToOrder>
    {
        public ConsumeChatCommandAddKitchenProductsToOrderValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.OrderId).NotEmpty().WithMessage("OrderId field is required");
            RuleFor(v => v.Command.KitchenProducts).NotEmpty().WithMessage("Products field is required");
            RuleForEach(v => v.Command.KitchenProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductId).NotEmpty().WithMessage("KitchenProductId field is required");
                //var invalidQuantityMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_recipes" });
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity field is required");
            });
        }
    }
}
