using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateKitchenProductCommandValidator : AbstractValidator<CreateKitchenProductCommand>
    {
        public CreateKitchenProductCommandValidator()
        {
            RuleFor(x => x.Name).MaximumLength(200).NotEmpty();
        }
    }
}
