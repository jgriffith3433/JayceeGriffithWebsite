using ContainerNinja.Contracts.DTO;
using ContainerNinja.Core.Handlers.Commands;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class UpdateKitchenProductCommandValidator : AbstractValidator<UpdateKitchenProductCommand>
    {
        public UpdateKitchenProductCommandValidator()
        {
            //RuleFor(x => x.Title).NotEmpty().WithMessage("Title field is required");
            //RuleFor(x => x.Color).NotEmpty().WithMessage("Tag a colorCode to the TodoList");
        }
    }
}
