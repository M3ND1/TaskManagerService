using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories;

public class RefreshTokenRepository(TaskManagerDbContext dbContext, TokenValidationParameters tokenValidationParameters) : IRefreshTokenRepository
{
    private readonly TaskManagerDbContext _dbContext = dbContext;
    private readonly TokenValidationParameters _tokenValidationParameters = tokenValidationParameters;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) => await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task<bool> IsValidAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens.AsNoTracking().AnyAsync(t => t.Token == token && !t.Invalidated, cancellationToken);
    }

    public async Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        int revokedCount = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.Invalidated)
            .ExecuteUpdateAsync(rt => rt.SetProperty(t => t.Invalidated, true), cancellationToken);
        return revokedCount > 0;
    }

    public async Task<bool> SaveAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        if (refreshToken == null)
            return false;

        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string accessToken)
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

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        => validatedToken is JwtSecurityToken jwtSecurityToken
       && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

    public async Task<bool> ValidateRefreshTokenAsync(string oldRefreshToken, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken = default)
    {
        var isValidRefreshToken = await IsValidAsync(oldRefreshToken, cancellationToken);
        if (!isValidRefreshToken)
            return false;

        if (claimsPrincipal == null)
            return false;

        RefreshToken? storedRefreshToken = await GetByTokenAsync(oldRefreshToken, cancellationToken);
        bool isValid = ValidateStoredRT(storedRefreshToken, claimsPrincipal);


        return true;
    }

    private static bool ValidateStoredRT(RefreshToken? storedRefreshToken, ClaimsPrincipal claimsPrincipal)
    {
        if (storedRefreshToken is null)
            return false;

        if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
            return false;

        if (storedRefreshToken.Invalidated)
            return false;

        var jti = claimsPrincipal.Claims
            .SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(jti))
            return false;

        return true;
    }
}
