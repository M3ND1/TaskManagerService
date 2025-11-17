using AutoMapper;
using TaskManager.Application.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services;

public class UserService(IUserRepository userRepository, IMapper mapper, IPasswordService passwordService, IRefreshTokenRepository refreshTokenRepository)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    public async Task<UserResponseDto?> CreateUserAsync(CreateUserDto userDto, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(userDto.Email, cancellationToken))
            return null;

        User user = _mapper.Map<User>(userDto);
        user.PasswordHash = _passwordService.SecurePassword(userDto.Password);
        return await _userRepository.AddAsync(user, cancellationToken) == true
            ? _mapper.Map<UserResponseDto>(user)
            : null;
    }

    public async Task<UserResponseDto?> GetUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        return user == null ? null : _mapper.Map<UserResponseDto>(user);
    }

    public async Task<bool> CheckIfEmailExists(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.EmailExistsAsync(email, cancellationToken);
    }

    public async Task<UserResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (await CheckIfEmailExists(email, cancellationToken) == false) return null;

        var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);

        return user != null ? _mapper.Map<UserResponseDto>(user) : null;
    }

    public async Task<string?> GetUserHashedPasswordByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserPasswordHashByUsernameAsync(username, cancellationToken);
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto userDto, CancellationToken cancellationToken = default)
    {
        if (id < 0) return false;
        var user = await _userRepository.GetAsync(id, cancellationToken);
        if (user == null) return false;

        _mapper.Map(userDto, user);
        user.UpdatedAt = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetAsync(id, cancellationToken);
        return user == null ? false : await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task SaveRefreshTokenAsync(int userId, string refreshTokenString, CancellationToken cancellationToken)
    {
        var refreshTokenDto = new RefreshTokenDto()
        {
            UserId = userId,
            Token = refreshTokenString,
            ExpiresAt = DateTime.Now.AddDays(5),
            Invalidated = false,
        };
        RefreshToken refreshToken = _mapper.Map<RefreshToken>(refreshTokenDto);
        await _refreshTokenRepository.SaveAsync(refreshToken);
    }
}
