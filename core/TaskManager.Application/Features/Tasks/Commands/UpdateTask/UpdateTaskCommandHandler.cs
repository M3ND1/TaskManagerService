using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler(IManagedTaskRepository managedTaskRepository, IMapper mapper) : IRequestHandler<UpdateTaskCommand, ManagedTaskResponseDto>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<ManagedTaskResponseDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskFromDb = await _managedTaskRepository.GetAsync(request.TaskId, cancellationToken) 
            ?? throw new NotFoundException($"Task with ID {request.TaskId} not found");

        _mapper.Map(request.UpdateManagedTaskDto, taskFromDb);
        taskFromDb.UpdatedAt = DateTime.UtcNow;
        if (!await _managedTaskRepository.UpdateAsync(taskFromDb, cancellationToken))
            throw new DatabaseOperationException("Failed to update task in database");

        return _mapper.Map<ManagedTaskResponseDto>(taskFromDb);
    }
}
