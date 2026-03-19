using MediatR;
using AutoMapper;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Commands.CreateTag;

public class CreateTagCommandHandler(ITagRepository tagRepository, IMapper mapper) : IRequestHandler<CreateTagCommand, TagResponseDto>
{
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<TagResponseDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        Tag tag = _mapper.Map<Tag>(request.CreateTagDto);
        tag.CreatedById = request.UserId;

        if (!await _tagRepository.AddAsync(tag, cancellationToken))
            throw new DatabaseOperationException("Failed to create tag in database");

        return _mapper.Map<TagResponseDto>(tag);
    }
}
