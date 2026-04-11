using FluentValidation.TestHelper;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Application.Validators;
using TaskManager.Core.Enums;

namespace Application.UnitTests.Validators;

public class CreateManagedTaskDtoValidatorTests
{
    private readonly CreateManagedTaskDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        var dto = new CreateManagedTaskDto
        {
            Title = "Fix login bug",
            Description = "Users cannot login",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = PriorityLevel.High,
            EstimatedHours = 4
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenTitleIsEmpty(string? title)
    {
        var dto = new CreateManagedTaskDto { Title = title!, Description = "Desc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Title);
    }

    [Fact]
    public void Should_Fail_WhenTitleExceeds200Characters()
    {
        var dto = new CreateManagedTaskDto { Title = new string('A', 201), Description = "Desc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenDescriptionIsEmpty(string? desc)
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = desc! };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Description);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionExceeds2000Characters()
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = new string('A', 2001) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Description);
    }

    [Fact]
    public void Should_Fail_WhenDueDateIsInThePast()
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = "Desc", DueDate = DateTime.UtcNow.AddDays(-1) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.DueDate);
    }

    [Fact]
    public void Should_Pass_WhenDueDateIsNull()
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = "Desc", DueDate = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(t => t.DueDate);
    }

    [Fact]
    public void Should_Fail_WhenEstimatedHoursIsZero()
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = "Desc", EstimatedHours = 0 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.EstimatedHours);
    }

    [Fact]
    public void Should_Fail_WhenEstimatedHoursExceeds500()
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = "Desc", EstimatedHours = 501 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.EstimatedHours);
    }

    [Fact]
    public void Should_Pass_WhenEstimatedHoursIsNull()
    {
        var dto = new CreateManagedTaskDto { Title = "Task", Description = "Desc", EstimatedHours = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(t => t.EstimatedHours);
    }
}
