using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchForRecipesWithKitchenProductsValidator : AbstractValidator<ConsumeChatCommandSearchForRecipesWithKitchenProducts>
    {
        public ConsumeChatCommandSearchForRecipesWithKitchenProductsValidator()
        {
            RuleFor(v => v.Command.KitchenProducts).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "get_kitchen_products" }));
            RuleForEach(v => v.Command.KitchenProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductId).NotEmpty().WithMessage("KitchenProductId field is required");
            });
        }
    }
}
