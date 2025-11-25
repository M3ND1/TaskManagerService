using MediatR;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler(IManagedTaskRepository managedTaskRepository) : IRequestHandler<DeleteTaskCommand>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _managedTaskRepository.GetAsync(request.TaskId, cancellationToken) 
            ?? throw new NotFoundException($"Task with ID {request.TaskId} not found");

        if (!await _managedTaskRepository.DeleteAsync(task.Id, cancellationToken))
            throw new DatabaseOperationException("Failed to delete task from database");

        return Unit.Value;
    }
}
