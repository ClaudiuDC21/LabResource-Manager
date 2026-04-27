using LabResource.VerticalApi.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.VerticalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login.Command command)
    {
        var result = await _mediator.Send(command);
        if (result == null)
        {
            return Unauthorized(new { Message = "invalid email or password" });
        }

        return Ok(result);
    }
}