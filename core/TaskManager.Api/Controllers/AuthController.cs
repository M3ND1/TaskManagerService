using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using TaskManager.Core.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserService userService, IPasswordService passwordService) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly IPasswordService _passwordService = passwordService;

        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (await _userService.CheckIfEmailExists(createUserDto.Email))
                return BadRequest(new { message = "Email already exists in database" });

            var result = await _userService.CreateUserAsync(createUserDto);
            if (result == null) return BadRequest(new { message = "Something went wrong while creating user" });

            return CreatedAtAction(nameof(GetUser), new { result.Id }, result);
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
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var hashedPassword = await _userService.GetUserHashedPasswordByUsernameAsync(userLoginDto.Username);
            if (hashedPassword == null)
                return BadRequest(new { message = "Invalid email or password." });

            if (!_passwordService.VerifyPassword(userLoginDto.Password!, hashedPassword))
                return BadRequest(new { message = "Invalid email or password." });

            // var token = GenerateJwtToken(user);
            // return Ok(new { "token", new { user.Id, user.Email, user.Username } });

            return Ok();
        }
    }
}