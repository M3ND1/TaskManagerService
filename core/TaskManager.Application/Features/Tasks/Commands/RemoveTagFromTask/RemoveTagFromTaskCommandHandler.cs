using MediatR;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Commands.RemoveTagFromTask;

public class RemoveTagFromTaskCommandHandler(IManagedTaskRepository managedTaskRepository)
    : IRequestHandler<RemoveTagFromTaskCommand>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;

    public async Task<Unit> Handle(RemoveTagFromTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _managedTaskRepository.GetWithTagsAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException($"Task with ID {request.TaskId} not found");

        if (task.CreatedById != request.UserId)
            throw new ForbiddenException("You are not allowed to modify this task");

        var tag = task.Tags?.FirstOrDefault(t => t.Id == request.TagId)
            ?? throw new NotFoundException($"Tag {request.TagId} is not assigned to this task");

        task.Tags!.Remove(tag);

        if (!await _managedTaskRepository.UpdateAsync(task, cancellationToken))
            throw new DatabaseOperationException("Failed to remove tag from task");

        return Unit.Value;
    }
}
