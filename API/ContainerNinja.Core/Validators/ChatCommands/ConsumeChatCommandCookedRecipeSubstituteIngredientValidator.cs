using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCookedRecipeSubstituteIngredientValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeIngredient>
    {
        public ConsumeChatCommandCookedRecipeSubstituteIngredientValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId field is required");
            RuleFor(v => v.Command.LoggedIngredientId).NotEmpty().WithMessage("LoggedIngredientId field is required");
            RuleFor(v => v.Command.NewIngredientName).NotEmpty().WithMessage("NewIngredientName field is required");
        }
    }
}
