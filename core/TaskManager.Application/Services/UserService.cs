using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using TaskManager.Application.DTOs;
using TaskManager.Core.Configuration;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services;

public class UserService(IUserRepository userRepository,
    IMapper mapper,
    IPasswordService passwordService,
    ITokenValidationService tokenValidationService,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IOptions<AuthConfiguration> authConfig)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenValidationService _tokenValidationService = tokenValidationService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly AuthConfiguration _authConfig = authConfig.Value;

    public async Task<UserResponseDto?> CreateUserAsync(CreateUserDto userDto, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(userDto.Email, cancellationToken))
            throw new BadRequestException($"Email '{userDto.Email}' is already taken.");

        if (await _userRepository.UsernameExistsAsync(userDto.Username, cancellationToken))
            throw new BadRequestException($"Username '{userDto.Username}' is already taken.");

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

    public async Task<bool> CheckIfEmailExists(string email, CancellationToken cancellationToken = default) => await _userRepository.EmailExistsAsync(email, cancellationToken);

    public async Task<UserResponseDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (await CheckIfEmailExists(email, cancellationToken) == false) return null;

        var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
        return user != null ? _mapper.Map<UserResponseDto>(user) : null;
    }

    public async Task<string?> GetUserHashedPasswordByEmailAsync(string email, CancellationToken cancellationToken = default) => await _userRepository.GetUserPasswordHashByEmailAsync(email, cancellationToken);

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
        return user != null && await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<int> SaveRefreshTokenAsync(int userId, string refreshTokenString, CancellationToken cancellationToken)
    {
        var refreshTokenDto = new RefreshTokenDto()
        {
            UserId = userId,
            Token = refreshTokenString,
            ExpiryDate = DateTime.UtcNow.AddDays(_authConfig.RefreshTokenExpirationInDays),
            Invalidated = false,
        };
        RefreshToken refreshToken = _mapper.Map<RefreshToken>(refreshTokenDto);
        await _refreshTokenRepository.SaveAsync(refreshToken, cancellationToken);
        return refreshToken.Id;
    }
    public async Task<RefreshTokenResponse?> RefreshTokenForUserAsync(string oldAccessToken, string oldRefreshToken, CancellationToken cancellationToken)
    {
        var claimsPrincipal = _tokenValidationService.GetPrincipalFromExpiredTokenAsync(oldAccessToken);
        if (claimsPrincipal == null)
            return null;

        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        var userEmailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var userRoleClaim = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        if (userRoleClaim == null || userEmailClaim == null || userIdClaim == null)
            return null;

        int userId = int.Parse(userIdClaim.Value);
        bool userExists = await _userRepository.UserExistsByUserId(userId, cancellationToken);
        if (!userExists)
            return null;

        var isValidRefresh = await _tokenValidationService.ValidateRefreshTokenAsync(oldRefreshToken, claimsPrincipal, cancellationToken);
        if (!isValidRefresh)
            return null;

        string newAccessToken = _jwtTokenGenerator.GenerateToken(userId, userEmailClaim, userRoleClaim);
        string newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        RefreshToken? oldToken = await _refreshTokenRepository.GetByTokenAsync(oldRefreshToken, cancellationToken);
        if (oldToken == null) return null;

        int newTokenId = await SaveRefreshTokenAsync(userId, newRefreshToken, cancellationToken);
        await _refreshTokenRepository.RevokeOldUserTokenAsync(userId, oldRefreshToken, newTokenId, cancellationToken);

        return new RefreshTokenResponse(newAccessToken, newRefreshToken);
    }
}
