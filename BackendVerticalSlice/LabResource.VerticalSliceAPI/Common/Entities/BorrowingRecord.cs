using LabResource.VerticalApi.Common.Enums;

namespace LabResource.VerticalApi.Common.Entities;

public class BorrowingRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LabAssetId { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public DateTime? ActualBorrowedAt { get; set; }
    public DateTime? ActualReturnedAt { get; set; }
    public BorrowingStatus Status { get; set; } = BorrowingStatus.Pending;
    public string? Remarks { get; set; }
    public User User { get; set; } = null!;
    public LabAsset LabAsset { get; set; } = null!;
}