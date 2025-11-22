using System.Security.Claims;
using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task<bool> SaveAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsValidAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> RevokeOldUserTokenAsync(int userId, string oldRefreshToken, int oldTokenId, CancellationToken cancellationToken = default);
}
