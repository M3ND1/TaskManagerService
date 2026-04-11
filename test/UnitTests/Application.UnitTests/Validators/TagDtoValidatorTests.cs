using FluentValidation.TestHelper;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Application.Validators;

namespace Application.UnitTests.Validators;

public class CreateTagDtoValidatorTests
{
    private readonly CreateTagDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        var dto = new CreateTagDto { Name = "Bug", Color = "#FF0000", Description = "Bug tag" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenNameIsEmpty(string? name)
    {
        var dto = new CreateTagDto { Name = name! };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Name);
    }

    [Fact]
    public void Should_Fail_WhenNameExceeds100Characters()
    {
        var dto = new CreateTagDto { Name = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Name);
    }

    [Theory]
    [InlineData("red")]
    [InlineData("#GG0000")]
    [InlineData("#FF00")]
    public void Should_Fail_WhenColorFormatIsInvalid(string color)
    {
        var dto = new CreateTagDto { Name = "Bug", Color = color };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Color);
    }

    [Fact]
    public void Should_Pass_WhenColorIsNull()
    {
        var dto = new CreateTagDto { Name = "Bug", Color = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(t => t.Color);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionExceeds150Characters()
    {
        var dto = new CreateTagDto { Name = "Bug", Description = new string('A', 151) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Description);
    }
}

public class UpdateTagDtoValidatorTests
{
    private readonly UpdateTagDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreNull()
    {
        var dto = new UpdateTagDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenNameExceeds100Characters()
    {
        var dto = new UpdateTagDto { Name = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Name);
    }

    [Fact]
    public void Should_Fail_WhenColorFormatIsInvalid()
    {
        var dto = new UpdateTagDto { Color = "not-hex" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(t => t.Color);
    }

    [Fact]
    public void Should_Pass_WhenValidColorProvided()
    {
        var dto = new UpdateTagDto { Color = "#00FF00" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(t => t.Color);
    }
}
