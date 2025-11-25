using MediatR;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Features.Users.Commands.LoginUser;

public record LoginUserCommand(UserLoginDto LoginDto) : IRequest<UserLoginResponseDto>;
