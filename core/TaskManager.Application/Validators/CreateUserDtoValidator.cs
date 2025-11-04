using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(u => u.Email)
                .ApplyEmailRules();

            RuleFor(u => u.Username)
                .ApplyUsernameRules();

            RuleFor(u => u.FirstName)
                .ApplyNameRules("First name");

            RuleFor(u => u.LastName)
                .ApplyNameRules("Last name");

            RuleFor(u => u.Password)
                .ApplyPasswordRules();

            RuleFor(u => u.ConfirmPassword)
                .NotEmpty()
                .Equal(u => u.Password).WithMessage("Passwords do not match.")
                .When(u => !string.IsNullOrEmpty(u.Password));

            RuleFor(u => u.PhoneNumber)
                .ApplyPhoneRules()
                .When(u => !string.IsNullOrEmpty(u.PhoneNumber));
        }
    }
}
