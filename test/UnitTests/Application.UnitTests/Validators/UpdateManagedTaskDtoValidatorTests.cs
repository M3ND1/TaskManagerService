using FluentValidation.TestHelper;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.Validators;
using TaskManager.Core.Enums;

namespace Application.UnitTests.Validators;

public class UpdateManagedTaskDtoValidatorTests
{
    private readonly UpdateManagedTaskDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        var dto = new UpdateManagedTaskDto
        {
            Title = "Updated title",
            Description = "Updated desc",
            Priority = PriorityLevel.Medium,
            EstimatedHours = 10,
            ActualHours = 5
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_WhenAllFieldsAreNull()
    {
        var dto = new UpdateManagedTaskDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenTitleExceeds200Characters()
    {
        var dto = new UpdateManagedTaskDto { Title = new string('A', 201) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Title);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionExceeds2000Characters()
    {
        var dto = new UpdateManagedTaskDto { Description = new string('A', 2001) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Description);
    }

    [Fact]
    public void Should_Fail_WhenDueDateIsInThePast()
    {
        var dto = new UpdateManagedTaskDto { DueDate = DateTime.UtcNow.AddDays(-1) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.DueDate);
    }

    [Fact]
    public void Should_Fail_WhenActualHoursIsNegative()
    {
        var dto = new UpdateManagedTaskDto { ActualHours = -1 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.ActualHours);
    }

    [Fact]
    public void Should_Fail_WhenActualHoursExceeds1000()
    {
        var dto = new UpdateManagedTaskDto { ActualHours = 1001 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.ActualHours);
    }

    [Fact]
    public void Should_Fail_WhenCompletedWithoutActualHours()
    {
        var dto = new UpdateManagedTaskDto { IsCompleted = true, ActualHours = null };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.ActualHours);
    }

    [Fact]
    public void Should_Fail_WhenCompletedWithZeroActualHours()
    {
        var dto = new UpdateManagedTaskDto { IsCompleted = true, ActualHours = 0 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.ActualHours);
    }

    [Fact]
    public void Should_Pass_WhenCompletedWithValidActualHours()
    {
        var dto = new UpdateManagedTaskDto { IsCompleted = true, ActualHours = 5 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(t => t.ActualHours);
    }
}
