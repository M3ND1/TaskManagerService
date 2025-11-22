using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(u => u.Email)
            .ApplyEmailRules();

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
