using LabResource.Application.DTOs.Auth;
using LabResource.Application.DTOs.Users;
using LabResource.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { Message = "invalid email or password" });
        }

        return Ok(response);
    }
}