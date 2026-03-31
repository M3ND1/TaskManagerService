using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tags.Queries.GetAllTags;

public class GetAllTagsQueryHandler(ITagRepository tagRepository, IMapper mapper)
    : IRequestHandler<GetAllTagsQuery, PagedResult<TagResponseDto>>
{
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<TagResponseDto>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var (tags, totalCount) = await _tagRepository.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = _mapper.Map<IEnumerable<TagResponseDto>>(tags);
        return new PagedResult<TagResponseDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
