using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
