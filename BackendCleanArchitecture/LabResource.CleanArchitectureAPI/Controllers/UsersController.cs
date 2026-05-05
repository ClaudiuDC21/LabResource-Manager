using LabResource.Application.DTOs.Users;
using LabResource.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var result = await _userService.RegisterUserAsync(request);
        return Ok(result);
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
        return Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        await _userService.UpdateUserAsync(id, request);
        return NoContent();
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> UpdatePassword(Guid id, [FromBody] UpdatePasswordRequest request)
    {
        await _userService.UpdatePasswordAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _userService.DeactivateUserAsync(id);
        return NoContent();
    }
}