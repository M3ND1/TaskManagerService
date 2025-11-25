using MediatR;

namespace TaskManager.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(int UserId) : IRequest;
