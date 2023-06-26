using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateCookedRecipeCalledIngredientDetailsCommandValidator : AbstractValidator<UpdateCookedRecipeCalledIngredientDetailsCommand>
    {
        public UpdateCookedRecipeCalledIngredientDetailsCommandValidator()
        {
            RuleFor(v => v.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
        }
    }
}
