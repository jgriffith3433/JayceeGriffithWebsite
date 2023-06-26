using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteCookedRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandDeleteCookedRecipeIngredient>
    {
        public ConsumeChatCommandDeleteCookedRecipeIngredientValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId field is required");
            RuleFor(v => v.Command.IngredientName).NotEmpty().WithMessage("IngredientName field is required");
        }
    }
}
