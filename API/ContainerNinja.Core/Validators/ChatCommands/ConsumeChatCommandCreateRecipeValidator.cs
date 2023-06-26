using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateRecipeValidator : AbstractValidator<ConsumeChatCommandCreateNewRecipe>
    {
        public ConsumeChatCommandCreateRecipeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
        }
    }
}
