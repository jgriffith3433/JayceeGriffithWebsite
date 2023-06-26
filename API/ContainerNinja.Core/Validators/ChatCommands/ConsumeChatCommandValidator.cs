using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandValidator : AbstractValidator<ConsumeChatCommand>
    {
        public ConsumeChatCommandValidator()
        {
            RuleFor(v => v.CurrentChatMessage)
                .NotEmpty().WithMessage("CurrentChatMessage is required.");

            RuleFor(v => v.CurrentUrl)
                .NotEmpty().WithMessage("CurrentUrl is required.");

            RuleFor(v => v.ChatMessages)
                .NotEmpty().WithMessage("ChatMessages is required.");

            RuleFor(v => v.ChatConversation)
                .NotEmpty().WithMessage("ChatConversation is required.");
        }
    }
}
