using MediatR;
using TaskManager.Application.DTOs.ManagedTask;

namespace TaskManager.Application.Features.Tasks.Commands.CreateTask;

public record CreateTaskCommand(CreateManagedTaskDto CreateManagedTaskDto, int UserId) : IRequest<ManagedTaskResponseDto>;
