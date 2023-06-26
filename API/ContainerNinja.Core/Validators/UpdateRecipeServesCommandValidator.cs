using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateRecipeServesCommandValidator : AbstractValidator<UpdateRecipeServesCommand>
    {
        public UpdateRecipeServesCommandValidator()
        {
            RuleFor(x => x.Serves).NotEmpty().WithMessage("Serves is required.");
        }
    }
}
