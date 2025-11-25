using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TaskManager.Application.DTOs;
using TaskManager.Core.Configuration;
using TaskManager.Core.Entities;
using TaskManager.Core.Exceptions;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandler(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, IPasswordService passwordService, IJwtTokenGenerator jwtTokenGenerator, IOptions<AuthConfiguration> authConfig) : IRequestHandler<LoginUserCommand, UserLoginResponseDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly AuthConfiguration _authConfig = authConfig.Value;
    public async Task<UserLoginResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var hashedPassword = await _userRepository.GetUserPasswordHashByEmailAsync(request.LoginDto.Email, cancellationToken);
        var superHardHash = _configuration.GetSection("SuperHardHash");
        var hashToVerify = hashedPassword ?? superHardHash["Secret"];

        bool isPasswordValid = _passwordService.VerifyPassword(request.LoginDto.Password, hashToVerify!);
        if (hashedPassword == null || !isPasswordValid)
            throw new BadRequestException("Invalid email or password.");

        var user = await _userRepository.GetUserByEmailAsync(request.LoginDto.Email, cancellationToken) ?? throw new BadRequestException("User does not exist");
        
        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email!, user.Role);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken()
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(_authConfig.RefreshTokenExpirationInDays),
            CreatedAt = DateTime.UtcNow,
            Invalidated = false,
        };
        await _refreshTokenRepository.SaveAsync(refreshTokenEntity, cancellationToken);

        return new UserLoginResponseDto(token, refreshToken);
    }
}
