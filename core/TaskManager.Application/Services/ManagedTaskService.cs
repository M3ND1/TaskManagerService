using TaskManager.Application;
using TaskManager.Core;
using TaskManager.Core.Entities;

public class ManagedTaskService : IManagedTaskRepository
{
    private readonly IManagedTaskRepository _managedTaskRepository;
    public ManagedTaskService(IManagedTaskRepository managedTaskRepository)
    {
        _managedTaskRepository = managedTaskRepository;
    }

    public Task AddAsync(Task task)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int taskId)
    {
        throw new NotImplementedException();
    }

    public Task GetTaskById(int taskId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ManagedTask>> GetUserTasks(int userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Task task)
    {
        throw new NotImplementedException();
    }
}