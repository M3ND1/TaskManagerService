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
        public async Task<bool> AddAsync(ManagedTask task)
        {
            try
            {
                await _dbContext.ManagedTasks.AddAsync(task);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong while adding task to db: ${ex.Message}");
            }
        }
        public async Task<ManagedTask?> GetAsync(int taskId)
        {
            try
            {
                return await _dbContext.ManagedTasks.FirstOrDefaultAsync(x => x.Id == taskId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occured while retrieving task from database: {ex.Message}");
            }
        }
        public async Task<bool> UpdateAsync(ManagedTask task)
        {
            try
            {
                var managedTask = await GetAsync(task.Id);
                if (managedTask == null)
                    return false;

                managedTask.Title = task.Title ?? managedTask.Title;
                managedTask.Description = task.Description ?? managedTask.Description;
                managedTask.UpdatedAt = task.UpdatedAt ?? managedTask.UpdatedAt;
                managedTask.DueDate = task.DueDate ?? managedTask.DueDate;
                managedTask.CompletedAt = task.CompletedAt ?? managedTask.CompletedAt;
                managedTask.Priority = task.Priority;
                managedTask.IsCompleted = task.IsCompleted;
                managedTask.EstimatedHours = task.EstimatedHours ?? managedTask.EstimatedHours;
                managedTask.ActualHours = task.ActualHours ?? managedTask.ActualHours;

                await _dbContext.ManagedTasks.Where(t => t.Id == managedTask.Id).ExecuteUpdateAsync(t => t
                    .SetProperty(t => t.Title, managedTask.Title)
                    .SetProperty(t => t.Description, managedTask.Description)
                    .SetProperty(t => t.UpdatedAt, managedTask.UpdatedAt)
                    .SetProperty(t => t.DueDate, managedTask.DueDate)
                    .SetProperty(t => t.CompletedAt, managedTask.CompletedAt)
                    .SetProperty(t => t.Priority, managedTask.Priority)
                    .SetProperty(t => t.IsCompleted, managedTask.IsCompleted)
                    .SetProperty(t => t.EstimatedHours, managedTask.EstimatedHours)
                    .SetProperty(t => t.ActualHours, managedTask.ActualHours)
                );
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Update failed: {ex.Message}");
            }
        }
        public async Task<bool> DeleteAsync(int taskId)
        {
            try
            {
                ManagedTask? searchedTask = await GetAsync(taskId);

                if (searchedTask == null)
                    return false;
                _dbContext.ManagedTasks.Remove(searchedTask);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong while deleting task from db: ${ex.Message}");
            }
        }
    }
}