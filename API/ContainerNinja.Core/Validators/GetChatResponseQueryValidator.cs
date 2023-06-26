using ContainerNinja.Core.Handlers.Commands;
using ContainerNinja.Core.Handlers.Queries;
using FluentValidation;
using OpenAI.ObjectModels;

namespace ContainerNinja.Core.Validators
{
    public class GetChatResponseQueryValidator : AbstractValidator<GetChatResponseQuery>
    {
        public GetChatResponseQueryValidator()
        {
            RuleFor(v => v.ChatMessages)
                .NotEmpty().WithMessage("ChatMessages field is required");

            RuleFor(v => v.ChatConversation)
                .NotEmpty().WithMessage("ChatConversation field is required");

            RuleFor(v => v.CurrentUrl)
                .NotEmpty().WithMessage("CurrentUrl field is required");

            /*
            0 = unknown cmd: edit-recipe-ingredient
            1 = Okay, I have updated your recipe.
            2 = unknown cmd: edit-recipe-ingredient <<----- Loop start
            */
            RuleFor(v => v.ChatMessages)
                .Must(cm =>
                {
                    if (cm.Count >= 3 && cm[cm.Count - 3].Content == cm[cm.Count - 1].Content)
                    {
                        if (cm[cm.Count - 3].From == StaticValues.ChatMessageRoles.System)
                        {
                            //Loop detected
                            return false;
                        }
                    }
                    return true;
                }).WithMessage("Could not negotiate a command");
        }
    }
}
