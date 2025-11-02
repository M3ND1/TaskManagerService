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
            //TODO: black listed passwords function check
            //TODO: Confirm password that matches password?

            RuleFor(u => u.PhoneNumber)
                .Length(9, 15).WithMessage("Phone number must be between 9 and 15 digits.")
                .Matches(@"^\d+$").WithMessage("Phone number can only contain digits.")
                .When(u => !string.IsNullOrEmpty(u.PhoneNumber));

        }
    }
}
