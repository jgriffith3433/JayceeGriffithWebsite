using ContainerNinja.Contracts.Enum;
using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandAddRecipeIngredientValidator : AbstractValidator<ConsumeChatCommandUpdateRecipe>
    {
        public ConsumeChatCommandAddRecipeIngredientValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage("RecipeId field is required");
            RuleFor(v => v.Command.KitchenProducts).NotEmpty().WithMessage("KitchenProducts field is required");
            RuleForEach(v => v.Command.KitchenProducts).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductId).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_kitchen_products" }));
                i.RuleFor(x => x.Quantity).NotEmpty().WithMessage("Must provide Quantity field");
                //i.RuleFor(x => x.UnitType).NotEmpty().WithMessage($"UnitType field is required. The available values are: {string.Join(", ", Enum.GetValues(typeof(KitchenUnitType)).Cast<KitchenUnitType>().Select(p => p.ToString()))}");
            });
        }
    }
}
