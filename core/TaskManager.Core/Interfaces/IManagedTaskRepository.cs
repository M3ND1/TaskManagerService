using TaskManager.Core.Entities;
using TaskManager.Core.Enums;

namespace TaskManager.Core.Interfaces;

public interface IManagedTaskRepository
{
    Task<bool> AddAsync(ManagedTask task, CancellationToken cancellationToken = default);
    Task<ManagedTask?> GetAsync(int taskId, CancellationToken cancellationToken = default);
    Task<ManagedTask?> GetWithTagsAsync(int taskId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<ManagedTask> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        bool? isCompleted = null, PriorityLevel? priority = null,
        int? assignedToId = null, string? search = null,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ManagedTask mappedTask, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int taskId, CancellationToken cancellationToken = default);
}

