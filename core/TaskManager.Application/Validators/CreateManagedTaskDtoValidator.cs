using FluentValidation;
using TaskManager.Application.DTOs;
using TaskManager.Core.Enums;

namespace TaskManager.Application.Validators
{
    public class CreateManagedTaskDtoValidator : AbstractValidator<CreateManagedTaskDto>
    {
        public CreateManagedTaskDtoValidator()
        {
            RuleFor(t => t.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .Length(1, 200).WithMessage("Task title must be between 1 and 200 characters.");

            RuleFor(t => t.Description)
                .NotEmpty().WithMessage("Task description is required.")
                .Length(1, 2000).WithMessage("Task description must be between 1 and 2000 characters.");

            RuleFor(t => t.DueDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
                .When(t => t.DueDate.HasValue);

            RuleFor(t => t.Priority)
                .IsInEnum().WithMessage("Priority must be a valid value (Low, Medium, High).");

            RuleFor(t => t.EstimatedHours)
                .GreaterThan(0).WithMessage("Estimated hours must be greater than 0.")
                .LessThanOrEqualTo(500).WithMessage("Actual hours must be less than 500 hours")
                .When(t => t.EstimatedHours.HasValue);
        }
    }
}
