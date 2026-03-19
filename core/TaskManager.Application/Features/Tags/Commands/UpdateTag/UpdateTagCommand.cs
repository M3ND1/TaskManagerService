using MediatR;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Commands.UpdateTag;

public record UpdateTagCommand(int TagId, UpdateTagDto UpdateTagDto) : IRequest<TagResponseDto>;
