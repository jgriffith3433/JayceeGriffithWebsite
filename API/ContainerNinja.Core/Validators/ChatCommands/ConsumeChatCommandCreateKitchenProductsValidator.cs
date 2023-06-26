using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateKitchenProductsValidator : AbstractValidator<ConsumeChatCommandCreateKitchenProducts>
    {
        public ConsumeChatCommandCreateKitchenProductsValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.KitchenProducts).NotEmpty().WithMessage("KitchenProducts field is required");
            RuleForEach(v => v.Command.KitchenProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductName).NotEmpty().WithMessage("KitchenProductName field is required");
                i.RuleFor(x => x.KitchenUnitType).NotEmpty().WithMessage("KitchenUnitType field is required");
            });
        }
    }
}
