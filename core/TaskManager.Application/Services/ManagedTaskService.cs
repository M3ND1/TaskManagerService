using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services;

public class ManagedTaskService
{
    private readonly IManagedTaskRepository _managedTaskRepository;
    private readonly IMapper _mapper;
    public ManagedTaskService(IManagedTaskRepository managedTaskRepository, IMapper mapper)
    {
        _managedTaskRepository = managedTaskRepository;
        _mapper = mapper;
    }
    public async Task<ManagedTaskResponseDto?> CreateTaskAsync(CreateManagedTaskDto managedTaskDto, int userId, int? assignedToId = null, CancellationToken cancellationToken = default)
    {
        ManagedTask managedTask = _mapper.Map<ManagedTask>(managedTaskDto);
        managedTask.AssignedToId = assignedToId;
        managedTask.CreatedById = userId;

        return await _managedTaskRepository.AddAsync(managedTask, cancellationToken) == true ? _mapper.Map<ManagedTaskResponseDto>(managedTask) : null;
    }

    public async Task<ManagedTaskResponseDto?> GetTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        ManagedTask? managedTask = await _managedTaskRepository.GetAsync(id, cancellationToken);
        return managedTask == null ? null : _mapper.Map<ManagedTaskResponseDto>(managedTask);
    }

    public async Task<bool> UpdateTaskAsync(int id, UpdateManagedTaskDto managedTaskDto, CancellationToken cancellationToken = default)
    {
        var taskFromDb = await _managedTaskRepository.GetAsync(id, cancellationToken);
        if (taskFromDb == null)
            return false;

        _mapper.Map(managedTaskDto, taskFromDb);
        taskFromDb.UpdatedAt = DateTime.UtcNow;
        return await _managedTaskRepository.UpdateAsync(taskFromDb, cancellationToken);
    }

    public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        ManagedTask? managedTask = await _managedTaskRepository.GetAsync(id, cancellationToken);
        if (managedTask == null) return false;

        return await _managedTaskRepository.DeleteAsync(id, cancellationToken);
    }
}
