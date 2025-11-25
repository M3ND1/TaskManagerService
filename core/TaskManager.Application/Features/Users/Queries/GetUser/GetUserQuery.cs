using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Queries.GetUser;

public record GetUserQuery(int UserId) : IRequest<UserResponseDto>;
