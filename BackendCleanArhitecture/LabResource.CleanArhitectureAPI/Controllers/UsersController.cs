using LabResource.Application.DTOs.Users;
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
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllActive()
    {
        var users = await _userService.GetAllActiveUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var success = await _userService.UpdateUserAsync(id, request);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _userService.DeactivateUserAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}