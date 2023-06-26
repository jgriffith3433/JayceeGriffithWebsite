using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandTemplateValidator : AbstractValidator<ConsumeChatCommandTemplate>
    {
        public ConsumeChatCommandTemplateValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.TemplateProperty).NotEmpty().WithMessage("TemplateProperty required");
        }
    }
}
