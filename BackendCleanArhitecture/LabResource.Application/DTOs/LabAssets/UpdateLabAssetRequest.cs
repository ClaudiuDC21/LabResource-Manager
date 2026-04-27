namespace LabResource.Application.DTOs.LabAssets;

public class UpdateLabAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Location { get; set; }
    public Guid? AssignedTeacherId { get; set; }
    public bool IsActive { get; set; }
}