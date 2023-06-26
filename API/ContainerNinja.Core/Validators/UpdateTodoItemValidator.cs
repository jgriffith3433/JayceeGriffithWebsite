using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateTodoItemValidator : AbstractValidator<UpdateTodoItemCommand>
    {
        public UpdateTodoItemValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title field is required");
        }
    }
}
