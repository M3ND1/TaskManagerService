using MediatR;
using TaskManager.Application.DTOs.ManagedTask;

namespace TaskManager.Application.Features.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand(int TaskId, UpdateManagedTaskDto UpdateManagedTaskDto) : IRequest<ManagedTaskResponseDto>;
