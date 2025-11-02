using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators
{
    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(u => u.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
