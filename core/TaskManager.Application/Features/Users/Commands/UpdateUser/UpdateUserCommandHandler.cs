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
        if (request.UserId < 0) throw new BadRequestException("Wrong user id");
        var user = await _userRepository.GetAsync(request.UserId, cancellationToken) ?? throw new BadRequestException("No user found");

        _mapper.Map(request.UserDto, user);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Unit.Value;
    }
}