using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeNameValidator : AbstractValidator<ConsumeChatCommandEditRecipeName>
    {
        public ConsumeChatCommandEditRecipeNameValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId field is required");
            RuleFor(v => v.Command.NewName).NotEmpty().WithMessage("NewName field is required");
            RuleFor(v => v.Command.Servings).NotEmpty().WithMessage("Servings field is required");
        }
    }
}
