using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateWalmartProductNameCommandValidator : AbstractValidator<UpdateProductNameCommand>
    {
        public UpdateWalmartProductNameCommandValidator()
        {
            RuleFor(x => x.Name).MaximumLength(200).NotEmpty();
        }
    }
}
