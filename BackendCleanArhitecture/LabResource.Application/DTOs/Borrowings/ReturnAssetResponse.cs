using LabResource.Domain.Enums;

namespace LabResource.Application.DTOs.Borrowings;

public class ReturnAssetResponse
{
    public Guid BorrowingRecordId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public DateTime ReturnedAt { get; set; }
    public AssetStatus NewStatus { get; set; }
}