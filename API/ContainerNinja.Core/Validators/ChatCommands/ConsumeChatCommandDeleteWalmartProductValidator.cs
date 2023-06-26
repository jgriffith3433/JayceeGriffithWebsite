using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteKitchenProductsValidator : AbstractValidator<ConsumeChatCommandDeleteKitchenProducts>
    {
        public ConsumeChatCommandDeleteKitchenProductsValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.KitchenProductsToDelete).NotEmpty().WithMessage("KitchenProductsToDelete field is required");
            RuleForEach(v => v.Command.KitchenProductsToDelete).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductId).NotEmpty().WithMessage("KitchenProductId field is required");
            });
        }
    }
}
