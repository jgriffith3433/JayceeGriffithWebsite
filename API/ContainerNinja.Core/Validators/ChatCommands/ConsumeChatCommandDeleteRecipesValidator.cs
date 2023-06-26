using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandDeleteRecipesValidator : AbstractValidator<ConsumeChatCommandDeleteRecipes>
    {
        public ConsumeChatCommandDeleteRecipesValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeIds).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_recipes" }));
        }
    }
}
