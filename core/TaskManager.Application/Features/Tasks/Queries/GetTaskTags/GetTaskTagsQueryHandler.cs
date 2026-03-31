using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskTags;

public class GetTaskTagsQueryHandler(IManagedTaskRepository managedTaskRepository, IMapper mapper)
    : IRequestHandler<GetTaskTagsQuery, IEnumerable<TagResponseDto>>
{
    private readonly IManagedTaskRepository _managedTaskRepository = managedTaskRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TagResponseDto>> Handle(GetTaskTagsQuery request, CancellationToken cancellationToken)
    {
        var task = await _managedTaskRepository.GetWithTagsAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException($"Task with ID {request.TaskId} not found");

        return _mapper.Map<IEnumerable<TagResponseDto>>(task.Tags ?? []);
    }
}
