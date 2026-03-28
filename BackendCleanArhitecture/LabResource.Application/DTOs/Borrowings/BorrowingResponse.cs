namespace LabResource.Application.DTOs.Borrowings;

public class BorrowingResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LabAssetId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}