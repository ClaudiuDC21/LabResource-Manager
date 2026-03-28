using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BorrowingsController : ControllerBase
{
    private readonly IBorrowingService _borrowingService;

    public BorrowingsController(IBorrowingService borrowingService)
    {
        _borrowingService = borrowingService;
    }

    [HttpPost("borrow")]
    public async Task<IActionResult> Borrow([FromBody] BorrowAssetRequest request)
    {
        try
        {
            var result = await _borrowingService.BorrowAssetAsync(request);
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
    public async Task<IActionResult> Return([FromBody] ReturnAssetRequest request)
    {
        try
        {
            var result = await _borrowingService.ReturnAssetAsync(request);
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
}