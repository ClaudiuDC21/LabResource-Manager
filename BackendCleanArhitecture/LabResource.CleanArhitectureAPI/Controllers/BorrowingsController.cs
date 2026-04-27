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

    [HttpPost("request")]
    public async Task<IActionResult> RequestAsset([FromBody] BorrowAssetRequest request)
    {
        try
        {
            var result = await _borrowingService.RequestAssetAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }

    [HttpPut("{borrowingId:guid}/review")]
    public async Task<IActionResult> ReviewRequest(Guid borrowingId, [FromBody] ReviewBorrowingRequest request)
    {
        try
        {
            await _borrowingService.ReviewRequestAsync(borrowingId, request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{borrowingId:guid}/pickup")]
    public async Task<IActionResult> PickUpAsset(Guid borrowingId)
    {
        try
        {
            await _borrowingService.PickUpAssetAsync(borrowingId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{borrowingId:guid}/return")]
    public async Task<IActionResult> Return(Guid borrowingId, [FromBody] ReturnAssetRequest request)
    {
        try
        {
            var result = await _borrowingService.ReturnAssetAsync(borrowingId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpGet("user/{userId:guid}/active")]
    public async Task<IActionResult> GetActiveForUser(Guid userId)
    {
        try
        {
            var result = await _borrowingService.GetActiveBorrowingsForUserAsync(userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpGet("asset/{assetId:guid}/history")]
    public async Task<IActionResult> GetAssetHistory(Guid assetId)
    {
        try
        {
            var result = await _borrowingService.GetAssetHistoryAsync(assetId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpGet("user/{userId:guid}/history")]
    public async Task<IActionResult> GetUserHistory(Guid userId)
    {
        try
        {
            var result = await _borrowingService.GetUserHistoryAsync(userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpGet("teacher/{teacherId:guid}/pending")]
    public async Task<IActionResult> GetPendingForTeacher(Guid teacherId)
    {
        try
        {
            var result = await _borrowingService.GetPendingRequestsForTeacherAsync(teacherId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}