using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>
    {
        public CreateTodoListCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title field is required");
            RuleFor(x => x.Color).NotEmpty().WithMessage("Color field is required");
        }
    }
}
