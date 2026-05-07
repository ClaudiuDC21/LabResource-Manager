using System.Security.Claims;
using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BorrowingsController : ControllerBase
{
    private readonly IBorrowingService _borrowingService;

    public BorrowingsController(IBorrowingService borrowingService)
    {
        _borrowingService = borrowingService;
    }

    [HttpPost]
    public async Task<IActionResult> RequestAsset([FromBody] BorrowAssetRequest request)
    {
        var result = await _borrowingService.RequestAssetAsync(request);
        return Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ReviewRequest(Guid id, [FromBody] ReviewBorrowingRequest request)
    {
        var teacherIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(teacherIdString, out Guid teacherId))
        {
            return Unauthorized(new { Message = "Invalid token format." });
        }

        await _borrowingService.ReviewRequestAsync(id, teacherId, request);
        return NoContent();
    }

    [HttpPost("{id:guid}/pickup")]
    public async Task<IActionResult> PickUpAsset(Guid id)
    {
        await _borrowingService.PickUpAssetAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnAsset(Guid id, [FromBody] ReturnAssetRequest request)
    {
        var result = await _borrowingService.ReturnAssetAsync(id, request);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}/active")]
    public async Task<IActionResult> GetActiveBorrowingsForUser(Guid userId)
    {
        var result = await _borrowingService.GetActiveBorrowingsForUserAsync(userId);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}/history")]
    public async Task<IActionResult> GetUserHistory(Guid userId)
    {
        var result = await _borrowingService.GetUserHistoryAsync(userId);
        return Ok(result);
    }

    [HttpGet("asset/{assetId:guid}/history")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetAssetHistory(Guid assetId)
    {
        var result = await _borrowingService.GetAssetHistoryAsync(assetId);
        return Ok(result);
    }

    [HttpGet("teacher/pending")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMyPendingRequests()
    {
        // Automatically fetch pending requests for the currently logged-in teacher
        var teacherIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(teacherIdString, out Guid teacherId))
        {
            return Unauthorized();
        }

        var result = await _borrowingService.GetPendingRequestsForTeacherAsync(teacherId);
        return Ok(result);
    }
}