using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserDto CreateUserDto) : IRequest<UserResponseDto>;
