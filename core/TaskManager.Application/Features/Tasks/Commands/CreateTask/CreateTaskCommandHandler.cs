using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler(IManagedTaskRepository managedTaskRepository, IMapper mapper) : IRequestHandler<CreateTaskCommand, ManagedTaskResponseDto>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<ManagedTaskResponseDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        ManagedTask managedTask = _mapper.Map<ManagedTask>(request.CreateManagedTaskDto);
        managedTask.AssignedToId = request.UserId;
        managedTask.CreatedById = request.UserId;

        if (!await _managedTaskRepository.AddAsync(managedTask, cancellationToken))
            throw new DatabaseOperationException("Failed to create task in database");

        return _mapper.Map<ManagedTaskResponseDto>(managedTask);
    }
}
