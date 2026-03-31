using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Queries.GetAllTags;

public record GetAllTagsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<TagResponseDto>>;
