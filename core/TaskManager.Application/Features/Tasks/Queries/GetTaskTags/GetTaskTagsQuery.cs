using MediatR;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tasks.Queries.GetTaskTags;

public record GetTaskTagsQuery(int TaskId) : IRequest<IEnumerable<TagResponseDto>>;
