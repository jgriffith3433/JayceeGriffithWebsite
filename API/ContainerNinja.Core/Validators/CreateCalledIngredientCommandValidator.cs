using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateCalledIngredientCommandValidator : AbstractValidator<CreateCalledIngredientCommand>
    {
        public CreateCalledIngredientCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(v => v.RecipeId).NotEmpty()
                .WithMessage("Recipe is required.");
        }
    }
}
