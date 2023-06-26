using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using System.Linq;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandSearchCookedRecipesValidator : AbstractValidator<ConsumeChatCommandSearchCookedRecipes>
    {
        public ConsumeChatCommandSearchCookedRecipesValidator()
        {
            //RuleFor(v => v.Command.Search).NotEmpty().WithMessage("Search field is required");
        }
    }
}
