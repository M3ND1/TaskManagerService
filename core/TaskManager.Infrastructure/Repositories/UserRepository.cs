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
        public async Task AddAsync(User user)
        {
            try
            {
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while adding the user {ex.Message}");
            }
        }
        public async Task<User> GetAsync(int id)
        {
            try
            {
                return await _dbContext.Users.FirstAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving the user {ex.Message}");
            }
        }
        public async Task UpdateAsync(User newUser)
        {
            try
            {
                await _dbContext.Users.ExecuteUpdateAsync(u =>
                u.SetProperty(p => p.Email, p => p.FirstName));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating the user {ex.Message}");
            }
        }
        public async Task DeleteAsync(int userId)
        {
            try
            {
                User user = await GetAsync(userId);
                if (user == null)
                    throw new Exception($"User not found in database");

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting the user {ex.Message}");
            }
        }
    }
}