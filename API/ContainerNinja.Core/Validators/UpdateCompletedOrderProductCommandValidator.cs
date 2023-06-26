using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateCompletedOrderProductCommandValidator : AbstractValidator<UpdateCompletedOrderProductCommand>
    {
        public UpdateCompletedOrderProductCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200)
                .NotEmpty();
        }
    }
}
