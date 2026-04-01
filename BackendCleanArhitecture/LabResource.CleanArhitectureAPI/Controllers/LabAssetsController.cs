using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Services;
using LabResource.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabResource.CleanApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LabAssetsController : ControllerBase
{
    private readonly ILabAssetService _labAssetService;

    public LabAssetsController(ILabAssetService labAssetService)
    {
        _labAssetService = labAssetService;
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Teacher))]
    public async Task<IActionResult> Create([FromBody] CreateLabAssetRequest request)
    {
        try
        {
            var result = await _labAssetService.CreateAssetAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllActive()
    {
        var assets = await _labAssetService.GetAllActiveAssetsAsync();
        return Ok(assets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var asset = await _labAssetService.GetAssetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        return Ok(asset);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Teacher))]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateLabAssetRequest request)
    {
        try
        {
            var success = await _labAssetService.UpdateAssetAsync(id, request);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Teacher))]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success = await _labAssetService.DeactivateAssetAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}