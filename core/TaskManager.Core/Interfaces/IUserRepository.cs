using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> AddAsync(User user);
        Task<User?> GetAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> IsEmailTakenByOtherUserAsync(string email, int userId);
        Task<string?> GetUserPasswordHashByUsernameAsync(string username);
        // Task<string> GetUserEmailByIdAsync(int userId);
        // Task<bool> IsEmailUnchangedAsync(string email, int userId);
    }
}