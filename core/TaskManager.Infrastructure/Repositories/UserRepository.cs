using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskManagerDbContext _dbContext;
        public UserRepository(TaskManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> AddAsync(User user)
        {
            try
            {
                if (user == null)
                    return false;

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while adding the user {ex.Message}");
            }
        }
        public async Task<User?> GetAsync(int id)
        {
            try
            {
                return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving the user {ex.Message}");
            }
        }
        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                if (user == null)
                    return false;
                if (!await _dbContext.Users.AnyAsync(u => u.Id == user.Id))
                    return false;

                int rowsAffected = await _dbContext.Users.Where(u => u.Id == user.Id).ExecuteUpdateAsync(u => u
                    .SetProperty(u => u.FirstName, user.FirstName)
                    .SetProperty(u => u.LastName, user.LastName)
                    .SetProperty(u => u.Email, user.Email)
                    .SetProperty(u => u.PhoneNumber, user.PhoneNumber)
                    .SetProperty(u => u.Username, user.Username)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow));
                await _dbContext.SaveChangesAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating the user {ex.Message}");
            }
        }
        public async Task<bool> DeleteAsync(int userId)
        {
            try
            {
                User? user = await GetAsync(userId);
                if (user == null)
                    return false;

                if (await _dbContext.ManagedTasks.AnyAsync(t => t.AssignedToId == userId || t.CreatedById == userId))
                {
                    await _dbContext.ManagedTasks.Where(t => t.AssignedToId == userId || t.CreatedById == userId).ExecuteDeleteAsync();
                }
                await _dbContext.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting the user {ex.Message}");
            }
        }
    }
}