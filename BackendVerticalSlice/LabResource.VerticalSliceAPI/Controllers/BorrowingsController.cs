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

    [HttpPost("borrow")]
    public async Task<IActionResult> Borrow([FromBody] BorrowAsset.Command command)
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

    [HttpPost("return")]
    public async Task<IActionResult> Return([FromBody] ReturnAsset.Command command)
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
}