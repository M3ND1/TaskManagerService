using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TaskManager.Application.DTOs.RefreshTokenDto;
using TaskManager.Application.Features.Token.Commands;
using TaskManager.Core.Configuration;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Features.Token;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ITokenValidationService> _tokenValidationServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly IOptions<AuthConfiguration> _authConfig;

    public RefreshTokenCommandHandlerTests()
    {
        _authConfig = Options.Create(new AuthConfiguration
        {
            Secret = "test-secret",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInHours = 1,
            RefreshTokenExpirationInDays = 7
        });
    }

    private RefreshTokenCommandHandler CreateHandler() => new(
        _userRepositoryMock.Object,
        _tokenValidationServiceMock.Object,
        _refreshTokenRepositoryMock.Object,
        _jwtTokenGeneratorMock.Object,
        _authConfig);

    private static ClaimsPrincipal CreateValidPrincipal(int userId = 1, string email = "john@example.com", string role = "User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public async Task Handle_Should_ReturnNewTokens_WhenRefreshIsValid()
    {
        // Arrange
        var principal = CreateValidPrincipal(userId: 1, email: "john@example.com", role: "User");
        var request = new RefreshTokenRequest("expired-jwt", "old-refresh-token");
        _tokenValidationServiceMock.Setup(s => s.GetPrincipalFromExpiredTokenAsync("expired-jwt")).Returns(principal);
        _userRepositoryMock.Setup(r => r.UserExistsByUserId(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenValidationServiceMock.Setup(s => s.ValidateRefreshTokenAsync("old-refresh-token", principal, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(1, "john@example.com", "User")).Returns("new-jwt");
        _jwtTokenGeneratorMock.Setup(g => g.GenerateRefreshToken()).Returns("new-refresh-token");
        _refreshTokenRepositoryMock.Setup(r => r.SaveAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _refreshTokenRepositoryMock.Setup(r => r.RevokeOldUserTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new RefreshTokenCommand(request), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-jwt");
        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_Should_RevokeOldToken_AndSaveNewToken()
    {
        // Arrange
        RefreshToken? capturedToken = null;
        var principal = CreateValidPrincipal(userId: 5);
        _tokenValidationServiceMock.Setup(s => s.GetPrincipalFromExpiredTokenAsync(It.IsAny<string>())).Returns(principal);
        _userRepositoryMock.Setup(r => r.UserExistsByUserId(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenValidationServiceMock.Setup(s => s.ValidateRefreshTokenAsync(It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns("jwt");
        _jwtTokenGeneratorMock.Setup(g => g.GenerateRefreshToken()).Returns("new-refresh");
        _refreshTokenRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((rt, _) => capturedToken = rt)
            .ReturnsAsync(true);
        _refreshTokenRepositoryMock.Setup(r => r.RevokeOldUserTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var handler = CreateHandler();

        // Act
        await handler.Handle(new RefreshTokenCommand(new RefreshTokenRequest("jwt", "old-refresh")), CancellationToken.None);

        // Assert
        capturedToken.Should().NotBeNull();
        capturedToken!.UserId.Should().Be(5);
        capturedToken.Token.Should().Be("new-refresh");
        capturedToken.Invalidated.Should().BeFalse();
        capturedToken.ExpiryDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
        _refreshTokenRepositoryMock.Verify(r => r.RevokeOldUserTokenAsync(5, "old-refresh", It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenAccessTokenIsInvalid()
    {
        // Arrange
        _tokenValidationServiceMock.Setup(s => s.GetPrincipalFromExpiredTokenAsync("garbage")).Returns((ClaimsPrincipal?)null);
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(new RefreshTokenCommand(new RefreshTokenRequest("garbage", "refresh")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Wrong token*");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenClaimsAreMissing()
    {
        // Arrange — principal without email claim
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1"), new(ClaimTypes.Role, "User") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        _tokenValidationServiceMock.Setup(s => s.GetPrincipalFromExpiredTokenAsync(It.IsAny<string>())).Returns(principal);
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(new RefreshTokenCommand(new RefreshTokenRequest("jwt", "refresh")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*claims*wrong*");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenUserDoesNotExist()
    {
        // Arrange
        var principal = CreateValidPrincipal(userId: 999);
        _tokenValidationServiceMock.Setup(s => s.GetPrincipalFromExpiredTokenAsync(It.IsAny<string>())).Returns(principal);
        _userRepositoryMock.Setup(r => r.UserExistsByUserId(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(new RefreshTokenCommand(new RefreshTokenRequest("jwt", "refresh")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*User does not exist*");
    }

    [Fact]
    public async Task Handle_Should_ThrowBadRequestException_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var principal = CreateValidPrincipal(userId: 1);
        _tokenValidationServiceMock.Setup(s => s.GetPrincipalFromExpiredTokenAsync(It.IsAny<string>())).Returns(principal);
        _userRepositoryMock.Setup(r => r.UserExistsByUserId(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _tokenValidationServiceMock.Setup(s => s.ValidateRefreshTokenAsync("bad-refresh", principal, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(new RefreshTokenCommand(new RefreshTokenRequest("jwt", "bad-refresh")), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Refresh token is not valid*");
    }
}
