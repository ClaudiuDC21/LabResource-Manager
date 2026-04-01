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

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> UpdatePassword(Guid id, [FromBody] UpdatePasswordRequest request)
    {
        try
        {
            var success = await _userService.UpdatePasswordAsync(id, request);

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
        var success = await _userService.DeactivateUserAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}