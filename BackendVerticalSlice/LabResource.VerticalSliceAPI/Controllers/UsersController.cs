using LabResource.VerticalApi.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.VerticalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login-or-register")]
    public async Task<IActionResult> LoginOrRegister([FromBody] LoginOrRegisterUser.Command command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
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