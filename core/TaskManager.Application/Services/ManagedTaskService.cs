using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services
{
    public class ManagedTaskService
    {
        private readonly IManagedTaskRepository _managedTaskRepository;
        private readonly IMapper _mapper;
        public ManagedTaskService(IManagedTaskRepository managedTaskRepository, IMapper mapper)
        {
            _managedTaskRepository = managedTaskRepository;
            _mapper = mapper;
        }
        public async Task<ManagedTaskResponseDto?> CreateTaskAsync(CreateManagedTaskDto managedTaskDto, int userId, int? assignedToId = null)
        {
            ManagedTask managedTask = _mapper.Map<ManagedTask>(managedTaskDto);
            managedTask.AssignedToId = assignedToId;
            managedTask.CreatedById = userId;

            return await _managedTaskRepository.AddAsync(managedTask) == true ? _mapper.Map<ManagedTaskResponseDto>(managedTask) : null;
        }
        public async Task<ManagedTaskResponseDto?> GetTaskAsync(int id)
        {
            ManagedTask? managedTask = await _managedTaskRepository.GetAsync(id);
            return managedTask == null ? null : _mapper.Map<ManagedTaskResponseDto>(managedTask);
        }
        public async Task<bool> UpdateTaskAsync(int id, UpdateManagedTaskDto managedTaskDto)
        {
            var managedTask = await _managedTaskRepository.GetAsync(id);
            if (managedTask == null)
                return false;

            _mapper.Map(managedTaskDto, managedTask);
            managedTask.UpdatedAt = DateTime.UtcNow;
            return await _managedTaskRepository.UpdateAsync(managedTask);
        }
        public async Task<bool> DeleteTaskAsync(int id)
        {
            ManagedTask? managedTask = await _managedTaskRepository.GetAsync(id);
            if (managedTask == null) return false;

            return await _managedTaskRepository.DeleteAsync(id);
        }
    }
}