using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using TaskManager.Application.DTOs;
using TaskManager.Core.Configuration;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Token.Commands;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenValidationService tokenValidationService,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IOptions<AuthConfiguration> authConfig) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenValidationService _tokenValidationService = tokenValidationService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly AuthConfiguration _authConfig = authConfig.Value;
    
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var claimsPrincipal = _tokenValidationService.GetPrincipalFromExpiredTokenAsync(request.RefreshTokenRequest.AccessToken) ?? throw new BadRequestException("Wrong token!");
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        var userEmailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var userRoleClaim = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        if (userRoleClaim == null || userEmailClaim == null || userIdClaim == null)
            throw new BadRequestException("User claims are wrong");

        int userId = int.Parse(userIdClaim.Value);
        bool userExists = await _userRepository.UserExistsByUserId(userId, cancellationToken);
        if (!userExists)
            throw new BadRequestException("User does not exist");

        var isValidRefresh = await _tokenValidationService.ValidateRefreshTokenAsync(request.RefreshTokenRequest.RefreshToken, claimsPrincipal, cancellationToken);
        if (!isValidRefresh)
            throw new BadRequestException("Refresh token is not valid");

        string newAccessToken = _jwtTokenGenerator.GenerateToken(userId, userEmailClaim, userRoleClaim);
        string newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken()
        {
            UserId = userId,
            Token = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(_authConfig.RefreshTokenExpirationInDays),
            CreatedAt = DateTime.UtcNow,
            Invalidated = false,
        };
        await _refreshTokenRepository.SaveAsync(refreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.RevokeOldUserTokenAsync(userId, request.RefreshTokenRequest.RefreshToken, refreshTokenEntity.Id, cancellationToken);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken);
    }
}