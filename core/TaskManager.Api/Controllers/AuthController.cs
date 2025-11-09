using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Core.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserService userService, IPasswordService passwordService, IJwtTokenGenerator jwtTokenGenerator, IConfiguration configuration) : ControllerBase
{
    private readonly UserService _userService = userService;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var userResponse = await _userService.CreateUserAsync(createUserDto);
        if (userResponse == null) return BadRequest(new { message = "Something went wrong while creating user" });

        return CreatedAtAction(nameof(GetUser), new { userResponse.Id }, userResponse);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        var userResponseDto = await _userService.GetUserAsync(id);
        if (userResponseDto == null)
            return NotFound(new { message = "User not found." });

        return Ok(userResponseDto);
    }
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid user date entered." });

        var hashedPassword = await _userService.GetUserHashedPasswordByUsernameAsync(userLoginDto.Email);
        var superHardHash = _configuration.GetSection("SuperHardHash");
        var hashToVerify = hashedPassword ?? superHardHash["Secret"];

        bool isPasswordValid = _passwordService.VerifyPassword(userLoginDto.Password, hashToVerify);
        if (hashedPassword == null || !isPasswordValid)
            return BadRequest(new { message = "Invalid email or password." });

        var user = await _userService.GetByEmailAsync(userLoginDto.Email);

        var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        //new table tokens
        // _userService.SaveRefreshTokenAsync(userResponse.Id, resfreshToken);

        return Ok(new { token, refreshToken });
    }
}
