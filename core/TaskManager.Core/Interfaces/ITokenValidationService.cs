using System.Security.Claims;

namespace TaskManager.Core.Interfaces;

public interface ITokenValidationService
{
    ClaimsPrincipal? GetPrincipalFromExpiredTokenAsync(string accessToken);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, ClaimsPrincipal principal, CancellationToken cancellationToken);
}