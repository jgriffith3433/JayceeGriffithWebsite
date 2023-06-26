using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchWalmartProductsForKitchenProductsValidator : AbstractValidator<ConsumeChatCommandSearchWalmartProductsForKitchenProduct>
    {
        public ConsumeChatCommandSearchWalmartProductsForKitchenProductsValidator()
        {
            RuleFor(v => v.Command.KitchenProductIds).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_kitchen_products" }));
        }
    }
}
