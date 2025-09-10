using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class ManagedTaskRepository : IManagedTaskRepository
    {
        private readonly TaskManagerDbContext _dbContext;
        public ManagedTaskRepository(TaskManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(ManagedTask task)
        {
            try
            {
                await _dbContext.AddAsync(task);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong while adding task to db: ${ex.Message}");
            }
        }
        public async Task<ManagedTask> GetAsync(int taskId)
        {
            try
            {
                return await _dbContext.ManagedTasks.FirstAsync(x => x.Id == taskId);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }
        public async Task UpdateAsync(ManagedTask task)
        {
            try
            {
                _dbContext.ManagedTasks.Update(task);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Update failed: {ex.Message}");
            }
        }
        public async Task DeleteAsync(int taskId)
        {
            try
            {
                ManagedTask searchedTask = await GetAsync(taskId);
                if (searchedTask == null)
                    throw new Exception($"Searched task not found in database.");

                _dbContext.ManagedTasks.Remove(searchedTask);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong while deleting task from db: ${ex.Message}");
            }
        }
    }
}