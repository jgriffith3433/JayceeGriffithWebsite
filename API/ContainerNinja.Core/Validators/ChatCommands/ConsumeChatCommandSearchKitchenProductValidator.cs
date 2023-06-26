using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchKitchenProductValidator : AbstractValidator<ConsumeChatCommandSearchKitchenProducts>
    {
        public ConsumeChatCommandSearchKitchenProductValidator()
        {
            //RuleFor(v => v.Command.ListOfNames).NotEmpty().WithMessage("ListOfNames is required");
        }
    }
}
