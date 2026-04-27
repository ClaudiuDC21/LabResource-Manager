using LabResource.Domain.Enums;

namespace LabResource.Application.DTOs.Borrowings;

public class UserBorrowingHistoryResponse
{
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public DateTime? ActualReturnedAt { get; set; }
    public BorrowingStatus Status { get; set; }
    public bool IsDefective { get; set; }
    public string? Remarks { get; set; }
}