using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> AddAsync(User user);
        Task<User?> GetAsync(int id);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        // Task<bool> IsEmailUnchangedAsync(string email, int userId);
        // Task<bool> EmailExistsAsync(string email);
        // Task<bool> IsEmailTakenByOtherUserAsync(string email, int userId);
        // Task<string> GetUserEmailByIdAsync(int userId);
    }
}