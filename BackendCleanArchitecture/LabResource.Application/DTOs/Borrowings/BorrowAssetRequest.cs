namespace LabResource.Application.DTOs.Borrowings;

public class BorrowAssetRequest
{
    public Guid UserId { get; set; }
    public Guid LabAssetId { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public DateTime RequestedEndDate { get; set; }
}