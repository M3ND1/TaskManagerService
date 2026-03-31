using FluentAssertions;
using FluentValidation.TestHelper;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;

namespace Application.UnitTests.Validators;

public class CreateUserDtoValidatorTests
{
    private readonly CreateUserDtoValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        var dto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Username = "johndoe",
            Password = "SecureP@ss1",
            ConfirmPassword = "SecureP@ss1",
            PhoneNumber = "123456789"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenEmailIsEmpty(string? email)
    {
        var dto = new CreateUserDto { Email = email!, Username = "user", Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Fact]
    public void Should_Fail_WhenEmailFormatIsInvalid()
    {
        var dto = new CreateUserDto { Email = "not-an-email", Username = "user", Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenUsernameIsEmpty(string? username)
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = username!, Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Username);
    }

    [Fact]
    public void Should_Fail_WhenUsernameTooShort()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "ab", Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Username);
    }

    [Fact]
    public void Should_Fail_WhenPasswordIsTooShort()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "P@1ab", ConfirmPassword = "P@1ab", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordMissingUppercase()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "p@ssword1", ConfirmPassword = "p@ssword1", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordMissingDigit()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "P@ssword!", ConfirmPassword = "P@ssword!", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordMissingSpecialChar()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "Password1", ConfirmPassword = "Password1", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordIsBlacklisted()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "password", ConfirmPassword = "password", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.Password);
    }

    [Fact]
    public void Should_Fail_WhenConfirmPasswordDoesNotMatch()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "P@ssword1", ConfirmPassword = "Different1!", FirstName = "A", LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.ConfirmPassword);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenFirstNameIsEmpty(string? name)
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = name!, LastName = "B" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.FirstName);
    }

    [Fact]
    public void Should_Fail_WhenPhoneNumberHasLetters()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = "A", LastName = "B", PhoneNumber = "12345abc" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(u => u.PhoneNumber);
    }

    [Fact]
    public void Should_Pass_WhenPhoneNumberIsNull()
    {
        var dto = new CreateUserDto { Email = "a@b.com", Username = "user", Password = "P@ssword1", ConfirmPassword = "P@ssword1", FirstName = "A", LastName = "B", PhoneNumber = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(u => u.PhoneNumber);
    }
}
