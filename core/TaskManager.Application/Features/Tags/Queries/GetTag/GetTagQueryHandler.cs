using MediatR;
using AutoMapper;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Queries.GetTag;

public class GetTagQueryHandler(ITagRepository tagRepository, IMapper mapper) : IRequestHandler<GetTagQuery, TagResponseDto>
{
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<TagResponseDto> Handle(GetTagQuery request, CancellationToken cancellationToken)
    {
        Tag tag = await _tagRepository.GetAsync(request.TagId, cancellationToken) 
            ?? throw new NotFoundException("Could not get tag");

        return _mapper.Map<TagResponseDto>(tag);
    }
}
