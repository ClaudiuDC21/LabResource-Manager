using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Services;
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
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateLabAssetRequest request)
    {
        var result = await _labAssetService.CreateAssetAsync(request);

        // Return 201 Created with a location header pointing to the GetById endpoint
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
        return Ok(asset);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLabAssetRequest request)
    {
        await _labAssetService.UpdateAssetAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _labAssetService.DeactivateAssetAsync(id);
        return NoContent();
    }
}