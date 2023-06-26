using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateCookedRecipeCommandValidator : AbstractValidator<CreateCookedRecipeCommand>
    {
        public CreateCookedRecipeCommandValidator()
        {
            RuleFor(v => v.RecipeId).NotEmpty()
                .WithMessage("Recipe is required.");
        }
    }
}
