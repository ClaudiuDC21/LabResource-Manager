using LabResource.Domain.Enums;

namespace LabResource.Application.DTOs.Borrowings;

public class ActiveBorrowingResponse
{
    public Guid BorrowingRecordId { get; set; }
    public Guid LabAssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? UserName { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public BorrowingStatus Status { get; set; } // Pentru a ști frontend-ul dacă e Pending sau Active
}