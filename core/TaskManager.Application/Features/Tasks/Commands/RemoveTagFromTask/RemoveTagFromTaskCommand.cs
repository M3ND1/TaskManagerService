using MediatR;

namespace TaskManager.Application.Features.Tasks.Commands.RemoveTagFromTask;

public record RemoveTagFromTaskCommand(int TaskId, int TagId, int UserId) : IRequest;
