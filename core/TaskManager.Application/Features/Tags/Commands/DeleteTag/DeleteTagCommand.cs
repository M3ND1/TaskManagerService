using MediatR;

namespace TaskManager.Application.Features.Tags.Commands.DeleteTag;

public record DeleteTagCommand(int TagId, int UserId) : IRequest;
