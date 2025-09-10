using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services
{
    public class ManagedTaskService
    {
        private readonly IManagedTaskRepository _managedTaskRepository;
        public ManagedTaskService(IManagedTaskRepository managedTaskRepository)
        {
            _managedTaskRepository = managedTaskRepository;
        }
        public async Task UpdateTaskAsync(int id, ManagedTaskDto managedTaskDto)
        {
            var managedTask = await _managedTaskRepository.GetAsync(id);

            if (managedTask == null)
                throw new Exception("Task not found in database");

            await _managedTaskRepository.UpdateAsync(managedTask);
        }
    }
}