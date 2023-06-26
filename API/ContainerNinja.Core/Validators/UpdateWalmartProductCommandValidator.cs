using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateWalmartProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateWalmartProductCommandValidator()
        {
            RuleFor(x => x.WalmartId).NotEmpty();
        }
    }
}
