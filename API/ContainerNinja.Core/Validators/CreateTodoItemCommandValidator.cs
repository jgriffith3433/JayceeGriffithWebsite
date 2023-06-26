using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
    {
        public CreateTodoItemCommandValidator()
        {
            RuleFor(x => x.ListId).NotEmpty().WithMessage("ListId field is required");
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title field is required");
        }
    }
}
