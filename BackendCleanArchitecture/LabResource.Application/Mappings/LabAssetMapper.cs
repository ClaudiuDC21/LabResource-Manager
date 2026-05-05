using LabResource.Application.DTOs.LabAssets;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;

namespace LabResource.Application.Mappings;

public static class LabAssetMapper
{
    public static LabAssetResponse ToResponse(this LabAsset asset)
    {
        var activeBorrowing = asset.BorrowingRecords?
            .FirstOrDefault(b => b.ActualReturnedAt == null && b.Status == BorrowingStatus.Active);

        return new LabAssetResponse
        {
            Id = asset.Id,
            Name = asset.Name,
            SerialNumber = asset.SerialNumber,
            Location = asset.Location,
            Status = asset.Status,
            IsActive = asset.IsActive,
            AssignedTeacherId = asset.AssignedTeacherId,
            AssignedTeacherName = asset.AssignedTeacher?.FullName,
            CurrentBorrowerName = activeBorrowing?.User?.FullName,
            CurrentBorrowDate = activeBorrowing?.ActualBorrowedAt
        };
    }
}