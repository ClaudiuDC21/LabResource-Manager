using LabResource.Application.DTOs.Borrowings;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;

namespace LabResource.Application.Mappings;

public static class BorrowingMapper
{
    public static BorrowingResponse ToBorrowingResponse(this BorrowingRecord record, string userName, string assetName)
    {
        return new BorrowingResponse
        {
            Id = record.Id,
            UserId = record.UserId,
            LabAssetId = record.LabAssetId,
            AssetName = assetName,
            UserName = userName,
            RequestedStartDate = record.RequestedStartDate,
            RequestedEndDate = record.RequestedEndDate,
            Status = record.Status
        };
    }

    public static ActiveBorrowingResponse ToActiveBorrowingResponse(this BorrowingRecord record)
    {
        return new ActiveBorrowingResponse
        {
            BorrowingRecordId = record.Id,
            LabAssetId = record.LabAssetId,
            AssetName = record.LabAsset.Name,
            SerialNumber = record.LabAsset.SerialNumber,
            RequestedStartDate = record.RequestedStartDate,
            RequestedEndDate = record.RequestedEndDate,
            Status = record.Status,
            UserName = record.User?.FullName
        };
    }

    public static AssetHistoryResponse ToAssetHistoryResponse(this BorrowingRecord record)
    {
        return new AssetHistoryResponse
        {
            BorrowingRecordId = record.Id,
            UserName = record.User.FullName,
            MatriculationNumber = record.User.MatriculationNumber,
            RequestedStartDate = record.RequestedStartDate,
            RequestedEndDate = record.RequestedEndDate,
            ActualReturnedAt = record.ActualReturnedAt,
            Status = record.Status,
            Remarks = record.Remarks
        };
    }

    public static UserBorrowingHistoryResponse ToUserBorrowingHistoryResponse(this BorrowingRecord record)
    {
        return new UserBorrowingHistoryResponse
        {
            AssetName = record.LabAsset.Name,
            SerialNumber = record.LabAsset.SerialNumber,
            RequestedStartDate = record.RequestedStartDate,
            RequestedEndDate = record.RequestedEndDate,
            ActualReturnedAt = record.ActualReturnedAt,
            Status = record.Status,
            IsDefective = record.LabAsset.Status == AssetStatus.Defective,
            Remarks = record.Remarks
        };
    }
}