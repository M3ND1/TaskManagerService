using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Authentication;

public class TokenValidationService(TokenValidationParameters tokenValidationParameters, IRefreshTokenRepository refreshTokenRepository) : ITokenValidationService
{
    private readonly TokenValidationParameters _tokenValidationParameters = tokenValidationParameters;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    public ClaimsPrincipal? GetPrincipalFromExpiredTokenAsync(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var tokenParameters = _tokenValidationParameters.Clone();
            tokenParameters.ValidateLifetime = false;
            var principal = tokenHandler.ValidateToken(accessToken, tokenParameters, out var validatedToken);
            return IsJwtWithValidSecurityAlgorithm(validatedToken) ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        RefreshToken? storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (storedRefreshToken == null || !IsValidStoredRefreshToken(storedRefreshToken))
            return false;

        return true;
    }
    private static bool IsValidStoredRefreshToken(RefreshToken? storedRefreshToken)
    {
        if (storedRefreshToken is null)
            return false;

        if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            return false;

        if (storedRefreshToken.Invalidated)
            return false;

        return true;
    }
    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        => validatedToken is JwtSecurityToken jwtSecurityToken
       && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
}
