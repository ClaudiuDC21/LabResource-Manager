using LabResource.Domain.Enums;

namespace LabResource.Application.DTOs.Borrowings;

public class BorrowingResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LabAssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
    public BorrowingStatus Status { get; set; }
}