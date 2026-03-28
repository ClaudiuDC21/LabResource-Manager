using LabResource.VerticalApi.Common.Enums;

namespace LabResource.VerticalApi.Common.Entities;

public class LabAsset
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? SerialNumber { get; set; }

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    public bool IsActive { get; set; } = true; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public ICollection<BorrowingRecord> BorrowingRecords { get; set; } = new List<BorrowingRecord>();
}