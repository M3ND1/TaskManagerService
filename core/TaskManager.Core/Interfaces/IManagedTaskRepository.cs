using TaskManager.Core.Entities;
using TaskManager.Core.Enums;

namespace TaskManager.Core.Interfaces
{
    public interface IManagedTaskRepository
    {
        Task<bool> AddAsync(ManagedTask task, int userId);
        Task<ManagedTask?> GetAsync(int taskId);
        Task<bool> UpdateAsync(ManagedTask task);
        Task<bool> DeleteAsync(int taskId);
        // Task<bool> ManagedTaskExistAsync(int taskId);
        // Task<bool> ManagedTaskDoneAsync(int taskId);
        // Task<IEnumerable<ManagedTask>> GetAllUserManagedTasksAsync(int userId);
        // Task<IEnumerable<ManagedTask>> GetTasksByPriorityAsync(int userId, PriorityLevel priority);
        // Task<IEnumerable<ManagedTask>> GetCompletedTasksAsync(int userId);
        // Task<IEnumerable<ManagedTask>> GetTasksDueByDateAsync(int userId, DateTime dueDate);
    }
}
