using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchRecipesValidator : AbstractValidator<ConsumeChatCommandSearchRecipes>
    {
        public ConsumeChatCommandSearchRecipesValidator()
        {
            //RuleFor(v => v.Command.Search).NotEmpty().WithMessage("Search field is required");
        }
    }
}
