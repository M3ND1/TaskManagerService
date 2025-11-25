using AutoMapper;
using MediatR;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId < 0) throw new BadRequestException("Invalid user ID");
        var user = await _userRepository.GetAsync(request.UserId, cancellationToken) 
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        _mapper.Map(request.UserDto, user);
        user.UpdatedAt = DateTime.UtcNow;
        if (!await _userRepository.UpdateAsync(user, cancellationToken))
            throw new DatabaseOperationException("Failed to update user in database");

        return Unit.Value;
    }
}