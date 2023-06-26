using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeIngredientKitchenUnitTypeValidator : AbstractValidator<ConsumeChatCommandEditRecipeIngredientKitchenUnitType>
    {
        public ConsumeChatCommandEditRecipeIngredientKitchenUnitTypeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId field is required");
            RuleFor(v => v.Command.IngredientId).NotEmpty().WithMessage("IngredientId field is required");
            RuleFor(v => v.Command.KitchenUnitType).NotEmpty().WithMessage($"KitchenUnitType field is required. The available values are: {string.Join(", ", Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().Select(p => p.ToString()))}");
        }
    }
}
