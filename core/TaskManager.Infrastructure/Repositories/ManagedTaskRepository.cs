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
        public async Task<bool> UpdateAsync(ManagedTask task)
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