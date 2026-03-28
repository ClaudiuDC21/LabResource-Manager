namespace LabResource.Application.DTOs.Borrowings;

public class ActiveBorrowingResponse
{
    public Guid BorrowingRecordId { get; set; }
    public Guid LabAssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime BorrowedAt { get; set; }
}