namespace LabResource.Application.DTOs.Borrowings;

public class ReturnAssetRequest
{
    public Guid LabAssetId { get; set; }
    public string? Remarks { get; set; }
    public bool IsDefective { get; set; }
}