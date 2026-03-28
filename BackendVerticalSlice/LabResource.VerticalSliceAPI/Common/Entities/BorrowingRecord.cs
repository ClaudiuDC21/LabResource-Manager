namespace LabResource.VerticalApi.Common.Entities;

public class BorrowingRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid LabAssetId { get; set; }

    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnedAt { get; set; }

    public string? Remarks { get; set; }


    public User User { get; set; } = null!;

    public LabAsset LabAsset { get; set; } = null!;
}