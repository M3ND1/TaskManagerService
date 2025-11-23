using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces;

public interface IUserRepository
{
    Task<bool> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> UserExistsByUserId(int userId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenByOtherUserAsync(string email, int userId, CancellationToken cancellationToken = default);
    Task<string?> GetUserPasswordHashByEmailAsync(string email, CancellationToken cancellationToken = default);
}
