using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController(ManagedTaskService managedTaskService) : ControllerBase
    {
        private readonly ManagedTaskService _managedTaskService = managedTaskService;

        [HttpPost]
        [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTask([FromBody] CreateManagedTaskDto createManagedTaskDto, int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            ManagedTaskResponseDto? result = await _managedTaskService.CreateTaskAsync(createManagedTaskDto, userId);
            if (result == null)
                return BadRequest(new { message = "Could not create task" });

            return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ManagedTaskResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTask(int id)
        {
            var managedTask = await _managedTaskService.GetTaskAsync(id);
            return managedTask != null ? Ok(managedTask) : NotFound(new { message = "Something went wrong" });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateManagedTaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateManagedTaskDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateManagedTaskDto updateManagedTaskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool success = await _managedTaskService.UpdateTaskAsync(id, updateManagedTaskDto);

            if (!success) return NotFound(new { message = "Task not found" });

            return Ok(new { message = "Task updated successfully!" });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask(int id)
        {
            bool success = await _managedTaskService.DeleteTaskAsync(id);
            if (!success)
                return NotFound(new { message = "Could not delete task with this id" });

            return NoContent();
        }
    }
}