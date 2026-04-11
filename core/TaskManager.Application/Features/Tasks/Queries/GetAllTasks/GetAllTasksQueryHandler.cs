using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Queries.GetAllTasks;

public class GetAllTasksQueryHandler(IManagedTaskRepository taskRepository, IMapper mapper)
    : IRequestHandler<GetAllTasksQuery, PagedResult<ManagedTaskResponseDto>>
{
    private readonly IManagedTaskRepository _taskRepository = taskRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<ManagedTaskResponseDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var (tasks, totalCount) = await _taskRepository.GetPagedAsync(
            request.PageNumber, request.PageSize,
            request.IsCompleted, request.Priority,
            request.AssignedToId, request.Search,
            cancellationToken);

        var items = _mapper.Map<IEnumerable<ManagedTaskResponseDto>>(tasks);
        return new PagedResult<ManagedTaskResponseDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
