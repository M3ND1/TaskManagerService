using MediatR;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository = userRepository;
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(request.UserId, cancellationToken) ?? throw new BadRequestException("User does not exist");
        await _userRepository.DeleteAsync(user.Id, cancellationToken);

        return Unit.Value;
    }
}
