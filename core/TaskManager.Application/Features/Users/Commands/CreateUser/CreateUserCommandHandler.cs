using AutoMapper;
using MediatR;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IMapper mapper) : IRequestHandler<CreateUserCommand, UserResponseDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IMapper _mapper = mapper;

    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.CreateUserDto.Email, cancellationToken))
            throw new BadRequestException($"Email '{request.CreateUserDto.Email}' is already taken.");

        if (await _userRepository.UsernameExistsAsync(request.CreateUserDto.Username, cancellationToken))
            throw new BadRequestException($"Username '{request.CreateUserDto.Username}' is already taken.");

        User user = _mapper.Map<User>(request.CreateUserDto);
        user.PasswordHash = _passwordService.SecurePassword(request.CreateUserDto.Password);
        if (!await _userRepository.AddAsync(user, cancellationToken))
            throw new DatabaseOperationException("Failed to create user in database");

        return _mapper.Map<UserResponseDto>(user);
    }
}
