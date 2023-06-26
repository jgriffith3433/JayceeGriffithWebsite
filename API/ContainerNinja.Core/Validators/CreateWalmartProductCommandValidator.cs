using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateWalmartProductCommandValidator : AbstractValidator<CreateWalmartProductCommand>
    {
        public CreateWalmartProductCommandValidator()
        {
            RuleFor(x => x.Name).MaximumLength(200).NotEmpty();
        }
    }
}
