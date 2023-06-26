using ContainerNinja.Core.Handlers.ChatCommands;
using FluentValidation;
using Newtonsoft.Json;

namespace ContainerNinja.Core.Validators.ChatCommands
{
    public class ConsumeChatCommandLinkKitchenProductsToWalmartProductsValidator : AbstractValidator<ConsumeChatCommandLinkKitchenProductToWalmartProduct>
    {
        public ConsumeChatCommandLinkKitchenProductsToWalmartProductsValidator()
        {
            var invalidWalmartIdMessage = @"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_walmart_products_for_kitchen_product" });
            //RuleFor(v => v.Command.UserGavePermission).Equal(true).WithMessage("ForceFunctionCall=none");

            RuleFor(v => v.Command.Links).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_kitchen_products" }));
            RuleForEach(v => v.Command.Links).ChildRules(i =>
            {
                i.RuleFor(x => x.KitchenProductId).NotEmpty().WithMessage(@"ForceFunctionCall=" + JsonConvert.SerializeObject(new { name = "search_kitchen_products" }));
                i.RuleFor(x => x.WalmartProductId).NotEmpty().WithMessage("WalmartProductId field is required");
                i.RuleFor(x => x.WalmartProductId).NotEqual(1).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(12).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(123).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(1234).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(12345).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(123456).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(1234567).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(12345678).WithMessage(invalidWalmartIdMessage);
                i.RuleFor(x => x.WalmartProductId).NotEqual(123456789).WithMessage(invalidWalmartIdMessage);
            });
        }
    }
}
