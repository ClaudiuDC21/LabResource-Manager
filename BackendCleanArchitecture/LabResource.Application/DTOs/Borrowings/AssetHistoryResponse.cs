using LabResource.Domain.Enums;

namespace LabResource.Application.DTOs.Borrowings;

public class AssetHistoryResponse
{
    public Guid BorrowingRecordId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? MatriculationNumber { get; set; }

    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public DateTime? ActualReturnedAt { get; set; }
    public BorrowingStatus Status { get; set; }
    public string? Remarks { get; set; }
}