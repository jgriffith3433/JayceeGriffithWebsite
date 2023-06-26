using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateRecipeNameCommandValidator : AbstractValidator<UpdateRecipeNameCommand>
    {
        public UpdateRecipeNameCommandValidator()
        {
            RuleFor(x => x.Name).MaximumLength(200)
                .WithMessage("Name must not exceed 200 characters.")
                .NotEmpty().WithMessage("Name is required.");
        }
    }
}
