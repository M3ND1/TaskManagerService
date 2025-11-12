using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Repositories
{
    public class ManagedTaskRepository(TaskManagerDbContext dbContext) : IManagedTaskRepository
    {
        private readonly TaskManagerDbContext _dbContext = dbContext;

        public async Task<bool> AddAsync(ManagedTask task, CancellationToken cancellationToken = default)
        {
            await _dbContext.ManagedTasks.AddAsync(task, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ManagedTask?> GetAsync(int taskId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ManagedTasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
        }

        public async Task<bool> UpdateAsync(ManagedTask mappedTask, CancellationToken cancellationToken = default)
        {
            _dbContext.ManagedTasks.Update(mappedTask);
            var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int taskId, CancellationToken cancellationToken = default)
        {
            ManagedTask? searchedTask = await GetAsync(taskId, cancellationToken);

            if (searchedTask == null)
                return false;
            _dbContext.ManagedTasks.Remove(searchedTask);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}