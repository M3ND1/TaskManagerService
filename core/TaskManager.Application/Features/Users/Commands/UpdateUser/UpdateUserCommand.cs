using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(int UserId, UpdateUserDto UserDto) : IRequest;
