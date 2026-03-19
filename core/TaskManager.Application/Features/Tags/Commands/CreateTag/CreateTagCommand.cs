using MediatR;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Commands.CreateTag;

public record CreateTagCommand(CreateTagDto CreateTagDto, int UserId) : IRequest<TagResponseDto>;
