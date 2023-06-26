using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateTodoListValidator : AbstractValidator<UpdateTodoListCommand>
    {
        public UpdateTodoListValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title field is required");
        }
    }
}
