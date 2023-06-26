using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateCompletedOrderCommandValidator : AbstractValidator<CreateCompletedOrderCommand>
    {
        public CreateCompletedOrderCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(v => v.UserImport)
                .MaximumLength(40000).WithMessage("UserImport must not exceed 40000 characters.");
        }
    }
}
