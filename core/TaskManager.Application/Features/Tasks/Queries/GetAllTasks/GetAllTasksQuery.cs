using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Application.DTOs.ManagedTask;
using TaskManager.Core.Enums;

namespace TaskManager.Application.Features.Tasks.Queries.GetAllTasks;

public record GetAllTasksQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? IsCompleted = null,
    PriorityLevel? Priority = null,
    int? AssignedToId = null,
    string? Search = null) : IRequest<PagedResult<ManagedTaskResponseDto>>;
