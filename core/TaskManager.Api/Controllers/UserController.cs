using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using System.Security.Claims;
using MediatR;
using TaskManager.Application.Features.Users.Commands.UpdateUser;
using TaskManager.Application.Features.Users.Commands.DeleteUser;
using TaskManager.Application.Features.Users.Commands.CreateUser;
using TaskManager.Application.Features.Users.Queries.GetUser;
using TaskManager.Application.Features.Users.Commands.LoginUser;
using TaskManager.Application.Features.Token.Commands;

namespace TaskManager.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(createUserDto);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { result.Id }, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
    {
        var query = new GetUserQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), updateUserDto);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(UserLoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto, CancellationToken cancellationToken)
    {        
        var command = new LoginUserCommand(userLoginDto);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenReuquest, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(refreshTokenReuquest);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
