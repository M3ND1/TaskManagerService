using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tags.Queries.GetAllTags;

public class GetAllTagsQueryHandler(ITagRepository tagRepository, IMapper mapper) : IRequestHandler<GetAllTagsQuery, IEnumerable<TagResponseDto>>
{
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TagResponseDto>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TagResponseDto>>(tags);
    }
}
