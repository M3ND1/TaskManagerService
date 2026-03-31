using FluentValidation;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Validators;

public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(t => t.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .Length(1, 100).WithMessage("Tag name must be between 1 and 100 characters.");

        RuleFor(t => t.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color code (e.g. #FF0000).")
            .When(t => !string.IsNullOrEmpty(t.Color));

        RuleFor(t => t.Description)
            .MaximumLength(150).WithMessage("Description must not exceed 150 characters.")
            .When(t => !string.IsNullOrEmpty(t.Description));
    }
}
