using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateCompletedOrderCommandValidator : AbstractValidator<UpdateCompletedOrderCommand>
    {
        public UpdateCompletedOrderCommandValidator()
        {
            RuleFor(v => v.UserImport)
                .MaximumLength(40000).WithMessage("UserImport must not exceed 40000 characters.");
        }
    }
}
