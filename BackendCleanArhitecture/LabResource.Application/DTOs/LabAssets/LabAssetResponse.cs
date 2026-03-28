using LabResource.Domain.Enums;

namespace LabResource.Application.DTOs.LabAssets;

public class LabAssetResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public AssetStatus Status { get; set; }
    public bool IsActive { get; set; }
}