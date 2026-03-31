using MediatR;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tasks.Commands.AssignTagToTask;

public record AssignTagToTaskCommand(int TaskId, int TagId, int UserId) : IRequest<IEnumerable<TagResponseDto>>;
