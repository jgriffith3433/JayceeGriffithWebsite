using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitType>
    {
        public ConsumeChatCommandEditCookedRecipeIngredientKitchenUnitTypeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId field is required");
            RuleFor(v => v.Command.LoggedIngredientId).NotEmpty().WithMessage("LoggedIngredientId field is required");
            RuleFor(v => v.Command.KitchenUnitType).NotEmpty().WithMessage($"KitchenUnitType field is required. The available values are: {string.Join(", ", Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().Select(p => p.ToString()))}");
        }
    }
}
