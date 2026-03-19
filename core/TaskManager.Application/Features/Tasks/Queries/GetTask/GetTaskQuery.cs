using MediatR;
using TaskManager.Application.DTOs.ManagedTask;

namespace TaskManager.Application.Features.Tasks.Queries.GetTask;

public record GetTaskQuery(int TaskId) : IRequest<ManagedTaskResponseDto>;
