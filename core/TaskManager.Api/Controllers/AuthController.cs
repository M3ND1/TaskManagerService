using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Exceptions.Custom;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Core.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, IPasswordService passwordService, IJwtTokenGenerator jwtGenerator, IConfiguration configuration) : ControllerBase
{
    private readonly UserService _userService = userService;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IJwtTokenGenerator _jwtGenerator = jwtGenerator;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        var userResponse = await _userService.CreateUserAsync(createUserDto, cancellationToken) ?? throw new BadRequestException("User registration failed");
        return CreatedAtAction(nameof(GetUser), new { userResponse.Id }, userResponse);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
    {
        var userResponseDto = await _userService.GetUserAsync(id, cancellationToken) ?? throw new NotFoundException("User not found.");
        return Ok(userResponseDto);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(UserLoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            throw new BadRequestException("Invalid user data entered.");

        var hashedPassword = await _userService.GetUserHashedPasswordByUsernameAsync(userLoginDto.Email, cancellationToken);
        var superHardHash = _configuration.GetSection("SuperHardHash");
        var hashToVerify = hashedPassword ?? superHardHash["Secret"];

        bool isPasswordValid = _passwordService.VerifyPassword(userLoginDto.Password, hashToVerify!);
        if (hashedPassword == null || !isPasswordValid)
            throw new BadRequestException("Invalid email or password.");

        var user = await _userService.GetByEmailAsync(userLoginDto.Email, cancellationToken) ?? throw new BadRequestException("User does not exist");
        var token = _jwtGenerator.GenerateToken(user.Id, user.Email!, user.Role);
        var refreshToken = _jwtGenerator.GenerateRefreshToken();

        await _userService.SaveRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        return Ok(new UserLoginResponseDto(token, refreshToken));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenReuquest, CancellationToken cancellationToken)
    {
        var (accessToken, refreshToken) = refreshTokenReuquest;

        var result = await _userService.RefreshTokenForUserAsync(accessToken, refreshToken, cancellationToken);

        if (result != null)
            return Ok(new RefreshTokenResponse(result.AccessToken, result.RefreshToken));

        return BadRequest();
    }
}
