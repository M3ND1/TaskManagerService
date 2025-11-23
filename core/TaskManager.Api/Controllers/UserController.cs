using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Core.Exceptions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateUserAsync(id, updateUserDto, cancellationToken);
        if (!result) throw new BadRequestException("Something went wrong while updating user account");

        return Ok(new { message = "User updated successfully!" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        bool result = await _userService.DeleteUserAsync(id, cancellationToken);
        if (!result) throw new BadRequestException("Could not find account with given id");

        return NoContent();
    }
}
