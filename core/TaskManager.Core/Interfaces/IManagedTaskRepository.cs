using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface IManagedTaskRepository
    {
        Task<bool> AddAsync(ManagedTask task, CancellationToken cancellationToken = default);
        Task<ManagedTask?> GetAsync(int taskId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(ManagedTask mappedTask, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int taskId, CancellationToken cancellationToken = default);
    }
}
