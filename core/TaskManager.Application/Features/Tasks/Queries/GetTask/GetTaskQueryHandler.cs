using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Queries.GetTask;

public class GetTaskQueryHandler(IManagedTaskRepository managedTaskRepository, IMapper mapper) : IRequestHandler<GetTaskQuery, ManagedTaskResponseDto>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<ManagedTaskResponseDto> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        ManagedTask managedTask = await _managedTaskRepository.GetAsync(request.TaskId, cancellationToken) 
            ?? throw new NotFoundException("Could not get task");

        return _mapper.Map<ManagedTaskResponseDto>(managedTask);
    }
}
