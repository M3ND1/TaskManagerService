using TaskManager.Core;
using TaskManager.Core.Entities;

namespace TaskManager.Application;

public interface IManagedTaskRepository
{
    Task<IEnumerable<ManagedTask>> GetUserTasks(int userId);
    Task GetTaskById(int taskId);
    Task AddAsync(Task task);
    Task UpdateAsync(Task task);
    Task DeleteAsync(int taskId);
}
