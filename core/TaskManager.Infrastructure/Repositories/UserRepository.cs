using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class UserRepository(TaskManagerDbContext dbContext) : IUserRepository
    {
        private readonly TaskManagerDbContext _dbContext = dbContext;
        public async Task<bool> AddAsync(User user)
        {
            if (user == null)
                return false;
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<User?> GetAsync(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email);
        }
        public async Task<bool> IsEmailTakenByOtherUserAsync(string email, int userId)
        {
            return await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == email && u.Id != userId);
        }
        public async Task<bool> UpdateAsync(User user)
        {
            //refactor to use Update only, business logic to be handled in service layer
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
        public async Task<bool> DeleteAsync(int userId)
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


        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users.AsNoTracking().Where(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<string?> GetUserPasswordHashByUsernameAsync(string username)
        {
            return await _dbContext.Users.AsNoTracking().Where(u => u.Username == username).Select(u => u.PasswordHash).FirstOrDefaultAsync();
        }
    }
}