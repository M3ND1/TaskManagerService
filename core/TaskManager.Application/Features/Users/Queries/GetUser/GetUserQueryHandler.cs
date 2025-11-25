using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserQuery, UserResponseDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    public async Task<UserResponseDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(request.UserId, cancellationToken);
        return _mapper.Map<UserResponseDto>(user) ?? throw new BadRequestException("Could not map user");
    }
}
