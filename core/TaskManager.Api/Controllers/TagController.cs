using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.DTOs.Tag;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Features.Tags.Queries.GetTag;
using TaskManager.Application.Features.Tags.Queries.GetAllTags;
using TaskManager.Application.Features.Tags.Commands.UpdateTag;
using TaskManager.Application.Features.Tags.Commands.CreateTag;
using TaskManager.Application.Features.Tags.Commands.DeleteTag;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createTagDto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in token"));

        var command = new CreateTagCommand(createTagDto, userId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTag), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TagResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTags([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetAllTagsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTag(int id, CancellationToken cancellationToken)
    {
        var query = new GetTagQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagDto updateTagDto, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in token"));
        var command = new UpdateTagCommand(id, updateTagDto, userId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found in token"));
        var command = new DeleteTagCommand(id, userId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
