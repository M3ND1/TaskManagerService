using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(u => u.Email)
                .EmailAddress().WithMessage("Please enter a valid email address.")
                .WithMessage("Please provide correct email ex: test@correctMail.com")
                .When(u => !string.IsNullOrEmpty(u.Email));

            RuleFor(u => u.Username)
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.")
                .Matches(ValidationRules.UsernamePattern).WithMessage("Username can only contain letters and numbers.")
                .When(u => !string.IsNullOrEmpty(u.Username));

            RuleFor(u => u.FirstName)
                .Length(1, 250).WithMessage("First name must be between 1 and 250 characters.")
                .Matches(ValidationRules.NamePattern).WithMessage("First name can only contain letters.")
                .When(u => !string.IsNullOrEmpty(u.FirstName));

            RuleFor(u => u.LastName)
                .Length(1, 250).WithMessage("Last name must be between 1 and 250 characters.")
                .Matches(ValidationRules.NamePattern).WithMessage("Last name can only contain letters.")
                .When(u => !string.IsNullOrEmpty(u.LastName));

            RuleFor(u => u.PhoneNumber)
                .Length(9, 15).WithMessage("Phone number must be between 9 and 15 digits.")
                .Matches(ValidationRules.PhonePattern).WithMessage("Phone number can only contain digits.")
                .When(u => !string.IsNullOrEmpty(u.PhoneNumber));
        }
    }
}
