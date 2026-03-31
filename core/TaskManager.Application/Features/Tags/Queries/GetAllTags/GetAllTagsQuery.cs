using MediatR;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Queries.GetAllTags;

public record GetAllTagsQuery : IRequest<IEnumerable<TagResponseDto>>;
