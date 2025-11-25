using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;
using MediatR;
using TaskManager.Application.Features.Tasks.Commands.CreateTask;
using TaskManager.Application.Features.Tasks.Queries.GetTask;
using TaskManager.Application.Features.Tasks.Commands.UpdateTask;
using TaskManager.Application.Features.Tasks.Commands.DeleteTask;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateManagedTaskDto createManagedTaskDto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in token"));

        var command = new CreateTaskCommand(createManagedTaskDto, userId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(int id, CancellationToken cancellationToken)
    {
        var query = new GetTaskQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateManagedTaskDto updateManagedTaskDto, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(id, updateManagedTaskDto);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
