using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Commands.AssignTagToTask;

public class AssignTagToTaskCommandHandler(
    IManagedTaskRepository managedTaskRepository,
    ITagRepository tagRepository,
    IMapper mapper)
    : IRequestHandler<AssignTagToTaskCommand, IEnumerable<TagResponseDto>>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TagResponseDto>> Handle(AssignTagToTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _managedTaskRepository.GetWithTagsAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException($"Task with ID {request.TaskId} not found");

        if (task.CreatedById != request.UserId)
            throw new ForbiddenException("You are not allowed to modify this task");

        var tag = await _tagRepository.GetAsync(request.TagId, cancellationToken)
            ?? throw new NotFoundException($"Tag with ID {request.TagId} not found");

        if (task.Tags?.Any(t => t.Id == request.TagId) == true)
            throw new BadRequestException($"Tag {request.TagId} is already assigned to this task");

        task.Tags ??= new List<Tag>();
        task.Tags.Add(tag);

        if (!await _managedTaskRepository.UpdateAsync(task, cancellationToken))
            throw new DatabaseOperationException("Failed to assign tag to task");

        return _mapper.Map<IEnumerable<TagResponseDto>>(task.Tags);
    }
}
