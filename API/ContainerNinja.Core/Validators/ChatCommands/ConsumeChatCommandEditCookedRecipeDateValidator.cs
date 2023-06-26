using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandEditCookedRecipeDateValidator : AbstractValidator<ConsumeChatCommandEditCookedRecipeDate>
    {
        public ConsumeChatCommandEditCookedRecipeDateValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId field is required");
            RuleFor(v => v.Command.When).NotEmpty().WithMessage("When field is required");
        }
    }
}
