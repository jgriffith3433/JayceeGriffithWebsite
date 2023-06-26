using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateCookedRecipeCalledIngredientCommandValidator : AbstractValidator<CreateCookedRecipeCalledIngredientCommand>
    {
        public CreateCookedRecipeCalledIngredientCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(v => v.CookedRecipeId).NotEmpty()
                .WithMessage("CookedRecipe is required.");
        }
    }
}
