using LabResource.Application.DTOs;
using LabResource.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login-or-register")]
    public async Task<IActionResult> LoginOrRegister([FromBody] LoginOrRegisterRequest request)
    {
        try
        {
            var result = await _userService.LoginOrRegisterAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            // Dacă dă eroare de la regula cu UBB, returnăm 400 Bad Request
            return BadRequest(new { Error = ex.Message });
        }
    }
}
