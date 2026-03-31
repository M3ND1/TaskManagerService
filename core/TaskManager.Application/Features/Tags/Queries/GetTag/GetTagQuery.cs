using MediatR;
using TaskManager.Application.DTOs.Tag;

namespace TaskManager.Application.Features.Tags.Queries.GetTag;

public record GetTagQuery(int TagId) : IRequest<TagResponseDto>;
