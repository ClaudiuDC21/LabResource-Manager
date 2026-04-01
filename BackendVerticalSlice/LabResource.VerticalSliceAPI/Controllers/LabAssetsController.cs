using LabResource.VerticalApi.Features.LabAssets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.VerticalApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LabAssetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabAssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLabAsset.Command command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllActive()
    {
        var result = await _mediator.Send(new GetAllActiveLabAssets.Query());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetLabAssetById.Query(id));
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLabAsset.Command command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        try
        {
            var success = await _mediator.Send(command);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _mediator.Send(new DeactivateLabAsset.Command(id));
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}