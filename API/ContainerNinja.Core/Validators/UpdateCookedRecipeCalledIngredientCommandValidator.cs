using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateCookedRecipeCalledIngredientCommandValidator : AbstractValidator<UpdateCookedRecipeCalledIngredientCommand>
    {
        public UpdateCookedRecipeCalledIngredientCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
        }
    }
}
