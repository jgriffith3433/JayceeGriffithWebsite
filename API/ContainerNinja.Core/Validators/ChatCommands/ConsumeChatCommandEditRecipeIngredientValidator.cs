using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandEditRecipeIngredient>
    {
        public ConsumeChatCommandEditRecipeIngredientValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId required");
            RuleFor(v => v.Command.IngredientId).NotEmpty().WithMessage("IngredientId required");
            RuleFor(v => v.Command.NewIngredientName).NotEmpty().WithMessage("NewIngredientName required");
        }
    }
}
