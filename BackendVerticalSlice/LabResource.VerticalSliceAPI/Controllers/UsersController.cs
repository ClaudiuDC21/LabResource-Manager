using LabResource.VerticalApi.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.VerticalApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllActive()
    {
        var result = await _mediator.Send(new GetAllActiveUsers.Query());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserById.Query(id));
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUser.Command command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var success = await _mediator.Send(command);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> UpdatePassword(Guid id, [FromBody] UpdatePassword.Command command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Error = "Id mismatch." });
        }

        try
        {
            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound(new { Message = "User not found." });
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
        var success = await _mediator.Send(new DeactivateUser.Command(id));
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}