using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories;

public class UserRepository(TaskManagerDbContext dbContext) : IUserRepository
{
    private readonly TaskManagerDbContext _dbContext = dbContext;

    public async Task<bool> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            return false;
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Username == username, cancellationToken);

    public async Task<bool> IsEmailTakenByOtherUserAsync(string email, int userId, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email && u.Id != userId, cancellationToken);

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            return false;
        _dbContext.Users.Update(user);
        int affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        User? user = await GetAsync(userId, cancellationToken);
        if (user == null)
            return false;

        if (await _dbContext.ManagedTasks.AnyAsync(t => t.AssignedToId == userId || t.CreatedById == userId, cancellationToken))
        {
            await _dbContext.ManagedTasks.Where(t => t.AssignedToId == userId || t.CreatedById == userId).ExecuteDeleteAsync(cancellationToken);
        }
        await _dbContext.Users.Where(u => u.Id == userId).ExecuteDeleteAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UserExistsByUserId(int userId, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().Where(u => u.Id == userId).AnyAsync(cancellationToken);

    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().Where(u => u.Username == username).FirstOrDefaultAsync(cancellationToken);

    public async Task<string?> GetUserPasswordHashByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().Where(u => u.Email == email).Select(u => u.PasswordHash).FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbContext.Users.AsNoTracking().Where(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);

}
