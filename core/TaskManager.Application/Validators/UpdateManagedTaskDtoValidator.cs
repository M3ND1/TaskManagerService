using FluentValidation;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Validators
{
    public class UpdateManagedTaskDtoValidator : AbstractValidator<UpdateManagedTaskDto>
    {
        public UpdateManagedTaskDtoValidator()
        {
            RuleFor(t => t.Title)
                .Length(1, 200).WithMessage("Task title must be between 1 and 200 characters.")
                .When(t => !string.IsNullOrEmpty(t.Title));

            RuleFor(t => t.Description)
                .Length(1, 2000).WithMessage("Task description must be between 1 and 2000 characters.")
                .When(t => !string.IsNullOrEmpty(t.Description));

            RuleFor(t => t.DueDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
                .When(t => t.DueDate.HasValue);

            RuleFor(t => t.Priority)
                .IsInEnum().WithMessage("Priority must be a valid value (Low, Medium, High).")
                .When(t => t.Priority.HasValue);

            RuleFor(t => t.EstimatedHours)
                .GreaterThan(0).WithMessage("Estimated hours must be greater than 0.")
                .LessThanOrEqualTo(500).WithMessage("Actual hours must be less than 500 hours")
                .When(t => t.EstimatedHours.HasValue);

            RuleFor(t => t.ActualHours)
                .GreaterThanOrEqualTo(0).WithMessage("Actual hours cannot be negative.")
                .LessThanOrEqualTo(1000).WithMessage("Actual hours must be less than 1000 hours.")
                .When(t => t.ActualHours.HasValue);

            //Business rules
            RuleFor(t => t.ActualHours)
                .NotNull().WithMessage("Actual hours are required when marking task as completed.")
                .GreaterThan(0).WithMessage("Actual hours must be greater than 0 when marking task as completed.")
                .When(t => t.IsCompleted == true);
        }
    }
}
