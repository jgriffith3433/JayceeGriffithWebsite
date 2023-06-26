using ContainerNinja.Contracts.DTO;
using FluentValidation;

namespace ContainerNinja.Core.Validators
{
    public class ValidateUserDTOValidator : AbstractValidator<ValidateUserDTO>
    {
        public ValidateUserDTOValidator()
        {
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessage("EmailAddress field is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password field is required");
            RuleFor(x => x.EmailAddress).EmailAddress().WithMessage("Not a valid EmailAddress");
        }
    }
}
