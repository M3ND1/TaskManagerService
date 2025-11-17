using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories;

public class RefreshTokenRepository(TaskManagerDbContext dbContext) : IRefreshTokenRepository
{
    private readonly TaskManagerDbContext _dbContext = dbContext;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) => await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task<bool> IsValidAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens.AsNoTracking().AnyAsync(t => t.Token == token && !t.Invalidated, cancellationToken);
    }

    public async Task<int> RevokeAllUserTokensAsync(int userId, CancellationToken cancellationToken = default)
    {
        int revokedCount = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.Invalidated)
            .ExecuteUpdateAsync(rt => rt.SetProperty(t => t.Invalidated, true), cancellationToken);
        return revokedCount;
    }

    public async Task<bool> SaveAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        if (refreshToken == null)
            return false;

        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
}