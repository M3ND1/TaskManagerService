using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data.Database;

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

        public async Task<ManagedTask?> GetWithTagsAsync(int taskId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ManagedTasks
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
        }

        public async Task<(IEnumerable<ManagedTask> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize,
            bool? isCompleted = null, PriorityLevel? priority = null,
            int? assignedToId = null, string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.ManagedTasks
                .AsNoTracking()
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (isCompleted.HasValue)
                query = query.Where(t => t.IsCompleted == isCompleted.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> UpdateAsync(ManagedTask mappedTask, CancellationToken cancellationToken = default)
        {
            if (mappedTask == null) return false;

            try
            {
                _dbContext.ManagedTasks.Update(mappedTask);
                var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
                return affectedRows > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
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