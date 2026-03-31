using FluentValidation.TestHelper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace Application.UnitTests.Validators;

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreNull()
    {
        var dto = new UpdateUserDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        var dto = new UpdateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Username = "johndoe",
            PhoneNumber = "123456789"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenEmailFormatIsInvalid()
    {
        var dto = new UpdateUserDto { Email = "bad-email" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Fact]
    public void Should_Fail_WhenUsernameTooShort()
    {
        var dto = new UpdateUserDto { Username = "ab" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Username);
    }

    [Fact]
    public void Should_Fail_WhenUsernameContainsSpecialChars()
    {
        var dto = new UpdateUserDto { Username = "user@name!" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Username);
    }

    [Fact]
    public void Should_Fail_WhenFirstNameContainsNumbers()
    {
        var dto = new UpdateUserDto { FirstName = "John123" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.FirstName);
    }

    [Fact]
    public void Should_Fail_WhenPhoneNumberHasLetters()
    {
        var dto = new UpdateUserDto { PhoneNumber = "12345abc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.PhoneNumber);
    }

    [Fact]
    public void Should_Fail_WhenPhoneNumberTooShort()
    {
        var dto = new UpdateUserDto { PhoneNumber = "12345" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.PhoneNumber);
    }
}
