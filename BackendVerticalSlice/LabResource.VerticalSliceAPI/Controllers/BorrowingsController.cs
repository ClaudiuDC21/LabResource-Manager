using LabResource.VerticalApi.Features.Borrowings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpPost]
    public async Task<IActionResult> RequestAsset([FromBody] RequestAsset.Command command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> ReviewRequest(Guid id, [FromBody] ReviewRequest.Command command)
    {
        var teacherIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(teacherIdString, out Guid teacherId))
        {
            return Unauthorized();
        }

        // Ne asigurăm că ID-ul din URL ajunge în comandă
        await _mediator.Send(command with { BorrowingId = id, TeacherId = teacherId });
        return NoContent();
    }

    [HttpPost("{id:guid}/pickup")]
    public async Task<IActionResult> PickUpAsset(Guid id)
    {
        await _mediator.Send(new PickUpAsset.Command(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnAsset(Guid id, [FromBody] ReturnAsset.Command command)
    {
        var result = await _mediator.Send(command with { BorrowingId = id });
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}/active")]
    public async Task<IActionResult> GetActiveBorrowingsForUser(Guid userId)
    {
        var result = await _mediator.Send(new GetActiveBorrowingsForUser.Query(userId));
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}/history")]
    public async Task<IActionResult> GetUserHistory(Guid userId)
    {
        var result = await _mediator.Send(new GetUserHistory.Query(userId));
        return Ok(result);
    }

    [HttpGet("asset/{assetId:guid}/history")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetAssetHistory(Guid assetId)
    {
        var result = await _mediator.Send(new GetAssetHistory.Query(assetId));
        return Ok(result);
    }

    [HttpGet("teacher/pending")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMyPendingRequests()
    {
        var teacherIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(teacherIdString, out Guid teacherId))
        {
            return Unauthorized();
        }

        var result = await _mediator.Send(new GetPendingRequestsForTeacher.Query(teacherId));
        return Ok(result);
    }
}