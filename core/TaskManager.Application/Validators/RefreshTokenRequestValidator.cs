using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.")
            .Must(BeValidJwtFormat).WithMessage("Invalid access token format.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .Length(88, 88).WithMessage("Invalid refresh token format.");
    }

    private bool BeValidJwtFormat(string token)
    {
        return !string.IsNullOrEmpty(token) && token.Split('.').Length == 3;
    }
}
