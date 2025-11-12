using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController(ManagedTaskService managedTaskService) : ControllerBase
{
    private readonly ManagedTaskService _managedTaskService = managedTaskService;

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateManagedTaskDto createManagedTaskDto, int userId, CancellationToken cancellationToken)
    {
        ManagedTaskResponseDto? result = await _managedTaskService.CreateTaskAsync(createManagedTaskDto, userId, userId, cancellationToken);
        if (result == null)
            return BadRequest(new { message = "Could not create task" });

        return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(int id, CancellationToken cancellationToken)
    {
        var managedTask = await _managedTaskService.GetTaskAsync(id, cancellationToken);
        return managedTask != null ? Ok(managedTask) : NotFound(new { message = "Something went wrong" });
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateManagedTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UpdateManagedTaskDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateManagedTaskDto updateManagedTaskDto, CancellationToken cancellationToken)
    {
        bool success = await _managedTaskService.UpdateTaskAsync(id, updateManagedTaskDto, cancellationToken);

        if (!success) return NotFound(new { message = "Task not found" });

        return Ok(new { message = "Task updated successfully!" });
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken)
    {
        bool success = await _managedTaskService.DeleteTaskAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { message = "Could not delete task with this id" });

        return NoContent();
    }
}
