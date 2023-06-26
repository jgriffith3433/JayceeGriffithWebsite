using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandCreateOrderValidator : AbstractValidator<ConsumeChatCommandCreateOrder>
    {
        public ConsumeChatCommandCreateOrderValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
        }
    }
}
