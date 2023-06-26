using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandUpdateLoggedRecipeValidator : AbstractValidator<ConsumeChatCommandUpdateLoggedRecipe>
    {
        public ConsumeChatCommandUpdateLoggedRecipeValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.LoggedRecipeId).NotEmpty().WithMessage("LoggedRecipeId field is required");
            RuleFor(v => v.Command.When).NotEmpty().WithMessage("When field is required");
            RuleFor(v => v.Command.Ingredients).NotEmpty().WithMessage("Ingredients field is required");
            RuleForEach(v => v.Command.Ingredients).ChildRules(i =>
            {
                i.RuleFor(x => x.IngredientName).NotEmpty().WithMessage("IngredientName field is required");
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Amount field is required");
                i.RuleFor(x => x.KitchenUnitType).NotEmpty().WithMessage($"KitchenUnitType field is required. The available values are: {string.Join(", ", Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().Select(p => p.ToString()))}");
            });
        }
    }
}
