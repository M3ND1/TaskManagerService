using FluentAssertions;
using TaskManager.Application.Services;

namespace Application.UnitTests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void SecurePassword_Should_ReturnNonEmptyHash()
    {
        // Act
        var hash = _passwordService.SecurePassword("SecureP@ss1");

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SecurePassword_Should_ReturnDifferentHashesForSamePassword()
    {
        // Act — different salts should produce different hashes
        var hash1 = _passwordService.SecurePassword("SecureP@ss1");
        var hash2 = _passwordService.SecurePassword("SecureP@ss1");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_Should_ReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "SecureP@ss1";
        var hash = _passwordService.SecurePassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_ReturnFalse_ForWrongPassword()
    {
        // Arrange
        var hash = _passwordService.SecurePassword("CorrectP@ss1");

        // Act
        var result = _passwordService.VerifyPassword("WrongP@ss1", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SecurePassword_Should_ReturnBase64EncodedString()
    {
        // Act
        var hash = _passwordService.SecurePassword("TestP@ss1");

        // Assert — base64 decode should not throw
        var act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }

    [Fact]
    public void SecurePassword_Should_ProduceHashWithSaltAndHashBytes()
    {
        // Act
        var hash = _passwordService.SecurePassword("TestP@ss1");
        var bytes = Convert.FromBase64String(hash);

        // Assert — 16 bytes salt + 32 bytes hash = 48 bytes total
        bytes.Should().HaveCount(48);
    }
}
