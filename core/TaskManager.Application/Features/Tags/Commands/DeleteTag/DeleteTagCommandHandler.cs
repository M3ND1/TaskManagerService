using MediatR;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Tags.Commands.DeleteTag;

public class DeleteTagCommandHandler(ITagRepository tagRepository) : IRequestHandler<DeleteTagCommand>
{
    private readonly ITagRepository _tagRepository = tagRepository;

    public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetAsync(request.TagId, cancellationToken) 
            ?? throw new NotFoundException($"Tag with ID {request.TagId} not found");

        if (!await _tagRepository.DeleteAsync(tag.Id, cancellationToken))
            throw new DatabaseOperationException("Failed to delete tag from database");

        return Unit.Value;
    }
}
