namespace LabResource.Application.DTOs.Borrowings;

public class AssetHistoryResponse
{
    public Guid BorrowingRecordId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? MatriculationNumber { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? Remarks { get; set; }
}