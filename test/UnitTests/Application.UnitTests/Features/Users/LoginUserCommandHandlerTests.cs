using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Features.Users.Commands.LoginUser;
using TaskManager.Core.Configuration;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Users;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly IOptions<AuthConfiguration> _authConfig;

    public LoginUserCommandHandlerTests()
    {
        _authConfig = Options.Create(new AuthConfiguration
        {
            Secret = "test-secret-key-that-is-long-enough-for-hmac",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInHours = 1,
            RefreshTokenExpirationInDays = 7
        });

        var superHardHashSection = new Mock<IConfigurationSection>();
        superHardHashSection.Setup(s => s["Secret"]).Returns("fake-super-hard-hash");
        _configurationMock.Setup(c => c.GetSection("SuperHardHash")).Returns(superHardHashSection.Object);
    }

    private LoginUserCommandHandler CreateHandler() => new(
        _userRepositoryMock.Object,
        _refreshTokenRepositoryMock.Object,
        _configurationMock.Object,
        _passwordServiceMock.Object,
        _jwtTokenGeneratorMock.Object,
        _authConfig);

    [Fact]
    public async Task Handle_Should_ReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User { Id = 1, Email = "john@example.com", Role = "User" };
        var dto = new UserLoginDto("john@example.com", "SecurePass1!");
        _userRepositoryMock.Setup(r => r.GetUserPasswordHashByEmailAsync("john@example.com", It.IsAny<CancellationToken>())).ReturnsAsync("stored_hash");
        _passwordServiceMock.Setup(p => p.VerifyPassword("SecurePass1!", "stored_hash")).Returns(true);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("john@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(1, "john@example.com", "User")).Returns("jwt-access-token");
        _jwtTokenGeneratorMock.Setup(g => g.GenerateRefreshToken()).Returns("refresh-token-value");
        _refreshTokenRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new LoginUserCommand(dto), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-access-token");
        result.RefreshToken.Should().Be("refresh-token-value");
    }

    [Fact]
    public async Task Handle_Should_SaveRefreshToken_WithCorrectExpiry()
    {
        // Arrange
        RefreshToken? capturedToken = null;
        var user = new User { Id = 5, Email = "john@example.com", Role = "User" };
        _userRepositoryMock.Setup(r => r.GetUserPasswordHashByEmailAsync("john@example.com", It.IsAny<CancellationToken>())).ReturnsAsync("hash");
        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), "hash")).Returns(true);
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync("john@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns("token");
        _jwtTokenGeneratorMock.Setup(g => g.GenerateRefreshToken()).Returns("refresh");
        _refreshTokenRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((rt, _) => capturedToken = rt)
            .ReturnsAsync(true);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new LoginUserCommand(new UserLoginDto("john@example.com", "Pass1!")), CancellationToken.None);

        // Assert
        capturedToken.Should().NotBeNull();
        capturedToken!.UserId.Should().Be(5);
        capturedToken.Token.Should().Be("refresh");
        capturedToken.Invalidated.Should().BeFalse();
        capturedToken.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenPasswordIsInvalid()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserPasswordHashByEmailAsync("john@example.com", It.IsAny<CancellationToken>())).ReturnsAsync("stored_hash");
        _passwordServiceMock.Setup(p => p.VerifyPassword("wrong_password", "stored_hash")).Returns(false);
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(new LoginUserCommand(new UserLoginDto("john@example.com", "wrong_password")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenEmailNotFound()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetUserPasswordHashByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), "fake-super-hard-hash")).Returns(false);
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(new LoginUserCommand(new UserLoginDto("nobody@example.com", "Pass1!")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_Should_UseTimingAttackMitigation_WhenUserNotFound()
    {
        // Arrange — when user doesn't exist, the handler should still call VerifyPassword with SuperHardHash
        _userRepositoryMock.Setup(r => r.GetUserPasswordHashByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        _passwordServiceMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var handler = CreateHandler();

        // Act
        try { await handler.Handle(new LoginUserCommand(new UserLoginDto("nobody@example.com", "Pass1!")), CancellationToken.None); } catch { }

        // Assert — VerifyPassword should still be called to prevent timing attacks
        _passwordServiceMock.Verify(p => p.VerifyPassword(It.IsAny<string>(), "fake-super-hard-hash"), Times.Once);
    }
}
