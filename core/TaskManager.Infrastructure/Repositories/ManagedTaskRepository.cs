using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class ManagedTaskRepository(TaskManagerDbContext dbContext) : IManagedTaskRepository
    {
        private readonly TaskManagerDbContext _dbContext = dbContext;

        public async Task<bool> AddAsync(ManagedTask task)
        {
            await _dbContext.ManagedTasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<ManagedTask?> GetAsync(int taskId)
        {
            return await _dbContext.ManagedTasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(x => x.Id == taskId);
        }
        public async Task<bool> UpdateAsync(ManagedTask mappedTask)
        {
            _dbContext.ManagedTasks.Update(mappedTask);
            var affectedRows = await _dbContext.SaveChangesAsync();
            return affectedRows > 0;
        }
        public async Task<bool> DeleteAsync(int taskId)
        {
            ManagedTask? searchedTask = await GetAsync(taskId);

            if (searchedTask == null)
                return false;
            _dbContext.ManagedTasks.Remove(searchedTask);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}