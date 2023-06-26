using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandLinkRecipeIngredientToKitchenProductValidator : AbstractValidator<ConsumeChatCommandLinkRecipeIngredientToKitchenProduct>
    {
        public ConsumeChatCommandLinkRecipeIngredientToKitchenProductValidator()
        {
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");

            var invalidRecipeIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_recipes" });
            RuleFor(v => v.Command.RecipeId).NotEmpty().WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(1).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(12).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(123).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(1234).WithMessage(invalidRecipeIdMessage);
            RuleFor(v => v.Command.RecipeId).NotEqual(12345).WithMessage(invalidRecipeIdMessage);

            var invalidKitchenProductIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_kitchen_products" });
            RuleFor(v => v.Command.KitchenProductId).NotEmpty().WithMessage(invalidKitchenProductIdMessage);
            RuleFor(v => v.Command.KitchenProductId).NotEqual(1).WithMessage(invalidKitchenProductIdMessage);
            RuleFor(v => v.Command.KitchenProductId).NotEqual(12).WithMessage(invalidKitchenProductIdMessage);
            RuleFor(v => v.Command.KitchenProductId).NotEqual(123).WithMessage(invalidKitchenProductIdMessage);
            RuleFor(v => v.Command.KitchenProductId).NotEqual(1234).WithMessage(invalidKitchenProductIdMessage);
            RuleFor(v => v.Command.KitchenProductId).NotEqual(12345).WithMessage(invalidKitchenProductIdMessage);

            var invalidIngredientIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "get_recipe_ingredients" });
            RuleFor(v => v.Command.IngredientId).NotEmpty().WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(1).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(12).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(123).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(1234).WithMessage(invalidIngredientIdMessage);
            RuleFor(v => v.Command.IngredientId).NotEqual(12345).WithMessage(invalidIngredientIdMessage);
        }
    }
}
