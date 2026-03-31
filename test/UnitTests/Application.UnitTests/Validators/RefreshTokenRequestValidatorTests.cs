using FluentValidation.TestHelper;
using TaskManager.Application.DTOs.RefreshTokenDto;
using TaskManager.Application.Validators;

namespace Application.UnitTests.Validators;

public class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_WhenAllFieldsAreValid()
    {
        // A valid JWT has 3 dot-separated parts; refresh token is 88 chars (base64 of 64 bytes)
        var validRefreshToken = Convert.ToBase64String(new byte[64]);
        var dto = new RefreshTokenRequest("header.payload.signature", validRefreshToken);
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenAccessTokenIsEmpty(string? accessToken)
    {
        var dto = new RefreshTokenRequest(accessToken!, "some-refresh-token-that-is-exactly-eighty-eight-characters-long-padded-to-fit-ok!");
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }

    [Fact]
    public void Should_Fail_WhenAccessTokenIsNotJwtFormat()
    {
        var dto = new RefreshTokenRequest("not-jwt-format", Convert.ToBase64String(new byte[64]));
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_WhenRefreshTokenIsEmpty(string? refreshToken)
    {
        var dto = new RefreshTokenRequest("header.payload.signature", refreshToken!);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void Should_Fail_WhenRefreshTokenLengthIsNot88()
    {
        var dto = new RefreshTokenRequest("header.payload.signature", "too-short");
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
