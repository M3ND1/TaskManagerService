using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs.Tag;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tags.Commands.UpdateTag;

public class UpdateTagCommandHandler(ITagRepository tagRepository, IMapper mapper) : IRequestHandler<UpdateTagCommand, TagResponseDto>
{
    private readonly ITagRepository _tagRepository = tagRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<TagResponseDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tagFromDb = await _tagRepository.GetAsync(request.TagId, cancellationToken)
            ?? throw new NotFoundException($"Tag with ID {request.TagId} not found");

        _mapper.Map(request.UpdateTagDto, tagFromDb);
        tagFromDb.UpdatedAt = DateTime.UtcNow;
        if (!await _tagRepository.UpdateAsync(tagFromDb, cancellationToken))
            throw new DatabaseOperationException("Failed to update tag in database");

        return _mapper.Map<TagResponseDto>(tagFromDb);
    }
}
