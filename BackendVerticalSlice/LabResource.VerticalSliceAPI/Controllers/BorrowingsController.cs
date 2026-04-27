using LabResource.VerticalApi.Features.Borrowings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.VerticalApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BorrowingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BorrowingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestAsset([FromBody] RequestAsset.Command command)
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }

    [HttpPut("{borrowingId:guid}/review")]
    public async Task<IActionResult> ReviewRequest(Guid borrowingId, [FromBody] ReviewRequest.Command command)
    {
        if (borrowingId != command.BorrowingId)
        {
            return BadRequest(new { Error = "Id mismatch." });
        }

        try
        {
            await _mediator.Send(command);
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
            await _mediator.Send(new PickUpAsset.Command(borrowingId));
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
    public async Task<IActionResult> Return(Guid borrowingId, [FromBody] ReturnAsset.Command command)
    {
        if (borrowingId != command.BorrowingId)
        {
            return BadRequest(new { Error = "Id mismatch." });
        }

        try
        {
            var result = await _mediator.Send(command);
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
            var result = await _mediator.Send(new GetActiveBorrowingsForUser.Query(userId));
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
            var result = await _mediator.Send(new GetAssetHistory.Query(assetId));
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
            var result = await _mediator.Send(new GetUserHistory.Query(userId));
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
            var result = await _mediator.Send(new GetPendingRequestsForTeacher.Query(teacherId));
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}