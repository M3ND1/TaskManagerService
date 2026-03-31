using FluentValidation.TestHelper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace Application.UnitTests.Validators;

public class UserLoginDtoValidatorTests
{
    private readonly UserLoginDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        var dto = new UserLoginDto("john@example.com", "P@ssword1");
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenEmailIsEmpty(string? email)
    {
        var dto = new UserLoginDto(email!, "password");
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Fact]
    public void Should_Fail_WhenEmailFormatIsInvalid()
    {
        var dto = new UserLoginDto("not-an-email", "password");
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenPasswordIsEmpty(string? password)
    {
        var dto = new UserLoginDto("a@b.com", password!);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }
}
