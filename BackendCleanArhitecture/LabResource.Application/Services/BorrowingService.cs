using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Interfaces.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;

namespace LabResource.Application.Services;

public class BorrowingService : IBorrowingService
{
    private readonly IUserRepository _userRepository;
    private readonly ILabAssetRepository _assetRepository;
    private readonly IBorrowingRecordRepository _borrowingRepository;

    public BorrowingService(
        IUserRepository userRepository,
        ILabAssetRepository assetRepository,
        IBorrowingRecordRepository borrowingRepository)
    {
        _userRepository = userRepository;
        _assetRepository = assetRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowingResponse> RequestAssetAsync(BorrowAssetRequest request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || !user.IsActive) throw new ArgumentException("User not found or inactive.");

        var asset = await _assetRepository.GetByIdAsync(request.LabAssetId);
        if (asset == null || !asset.IsActive) throw new ArgumentException("Asset not found or inactive.");

        bool hasOverlap = await _borrowingRepository.HasOverlappingReservationsAsync(asset.Id, request.RequestedStartDate, request.RequestedEndDate);
        if (hasOverlap)
        {
            throw new InvalidOperationException("The asset is already booked for the requested period.");
        }

        bool isAssignedTeacher = asset.AssignedTeacherId == user.Id;

        var borrowingRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LabAssetId = asset.Id,
            RequestedStartDate = request.RequestedStartDate,
            RequestedEndDate = request.RequestedEndDate,
            Status = isAssignedTeacher ? BorrowingStatus.Approved : BorrowingStatus.Pending
        };

        if (isAssignedTeacher)
        {
            asset.Status = AssetStatus.Borrowed;
        }
        else
        {
            asset.Status = AssetStatus.PendingApproval;
        }

        await _borrowingRepository.AddAsync(borrowingRecord);
        await _assetRepository.UpdateAsync(asset);
        await _borrowingRepository.SaveChangesAsync();

        return new BorrowingResponse
        {
            Id = borrowingRecord.Id,
            UserId = user.Id,
            LabAssetId = asset.Id,
            AssetName = asset.Name,
            UserName = user.FullName,
            RequestedStartDate = borrowingRecord.RequestedStartDate,
            RequestedEndDate = borrowingRecord.RequestedEndDate,
            Status = borrowingRecord.Status
        };
    }

    public async Task ReviewRequestAsync(Guid borrowingId, ReviewBorrowingRequest request)
    {
        var record = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (record == null) throw new ArgumentException("Borrowing record not found.");
        if (record.Status != BorrowingStatus.Pending) throw new InvalidOperationException("Only pending requests can be reviewed.");

        var asset = await _assetRepository.GetByIdAsync(record.LabAssetId);

        if (request.IsApproved)
        {
            record.Status = BorrowingStatus.Approved;
            record.Remarks = request.TeacherNotes;
        }
        else
        {
            record.Status = BorrowingStatus.Rejected;
            record.Remarks = request.TeacherNotes;
            asset!.Status = AssetStatus.Available;
            await _assetRepository.UpdateAsync(asset);
        }

        await _borrowingRepository.UpdateAsync(record);
        await _borrowingRepository.SaveChangesAsync();
    }

    public async Task PickUpAssetAsync(Guid borrowingId)
    {
        var record = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (record == null || record.Status != BorrowingStatus.Approved)
            throw new InvalidOperationException("Reservation is not approved yet.");

        var asset = await _assetRepository.GetByIdAsync(record.LabAssetId);

        record.Status = BorrowingStatus.Active;
        record.ActualBorrowedAt = DateTime.UtcNow;

        asset!.Status = AssetStatus.Borrowed;

        await _borrowingRepository.UpdateAsync(record);
        await _assetRepository.UpdateAsync(asset);
        await _borrowingRepository.SaveChangesAsync();
    }

    public async Task<ReturnAssetResponse> ReturnAssetAsync(Guid borrowingId, ReturnAssetRequest request)
    {
        var activeBorrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (activeBorrowing == null) throw new InvalidOperationException("Borrowing record not found.");

        var asset = await _assetRepository.GetByIdAsync(activeBorrowing.LabAssetId);

        activeBorrowing.Status = BorrowingStatus.Returned;
        activeBorrowing.ActualReturnedAt = DateTime.UtcNow;
        activeBorrowing.Remarks = string.IsNullOrEmpty(activeBorrowing.Remarks)
            ? request.Remarks
            : $"{activeBorrowing.Remarks} | Return Note: {request.Remarks}";

        asset!.Status = request.IsDefective ? AssetStatus.Defective : AssetStatus.Available;

        await _borrowingRepository.UpdateAsync(activeBorrowing);
        await _assetRepository.UpdateAsync(asset);
        await _borrowingRepository.SaveChangesAsync();

        return new ReturnAssetResponse
        {
            BorrowingRecordId = activeBorrowing.Id,
            AssetName = asset.Name,
            ReturnedAt = activeBorrowing.ActualReturnedAt.Value,
            NewStatus = asset.Status
        };
    }

    public async Task<IEnumerable<ActiveBorrowingResponse>> GetActiveBorrowingsForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new ArgumentException("User not found.");

        var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsByUserIdAsync(userId);

        return activeBorrowings.Select(b => new ActiveBorrowingResponse
        {
            BorrowingRecordId = b.Id,
            LabAssetId = b.LabAssetId,
            AssetName = b.LabAsset.Name,
            SerialNumber = b.LabAsset.SerialNumber,
            RequestedStartDate = b.RequestedStartDate,
            RequestedEndDate = b.RequestedEndDate,
            Status = b.Status
        });
    }

    public async Task<IEnumerable<AssetHistoryResponse>> GetAssetHistoryAsync(Guid labAssetId)
    {
        var asset = await _assetRepository.GetByIdAsync(labAssetId);
        if (asset == null) throw new ArgumentException("Asset not found.");

        var history = await _borrowingRepository.GetHistoryByAssetIdAsync(labAssetId);

        return history.Select(b => new AssetHistoryResponse
        {
            BorrowingRecordId = b.Id,
            UserName = b.User.FullName,
            MatriculationNumber = b.User.MatriculationNumber,
            RequestedStartDate = b.RequestedStartDate,
            RequestedEndDate = b.RequestedEndDate,
            ActualReturnedAt = b.ActualReturnedAt,
            Status = b.Status,
            Remarks = b.Remarks
        });
    }

    public async Task<IEnumerable<UserBorrowingHistoryResponse>> GetUserHistoryAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new ArgumentException("User not found.");

        var history = await _borrowingRepository.GetHistoryByUserIdAsync(userId);

        return history.Select(b => new UserBorrowingHistoryResponse
        {
            AssetName = b.LabAsset.Name,
            SerialNumber = b.LabAsset.SerialNumber,
            RequestedStartDate = b.RequestedStartDate,
            RequestedEndDate = b.RequestedEndDate,
            ActualReturnedAt = b.ActualReturnedAt,
            Status = b.Status,
            IsDefective = b.LabAsset.Status == AssetStatus.Defective,
            Remarks = b.Remarks
        });
    }

    public async Task<IEnumerable<ActiveBorrowingResponse>> GetPendingRequestsForTeacherAsync(Guid teacherId)
    {
        var pendingRequests = await _borrowingRepository.GetPendingRequestsForTeacherAsync(teacherId);

        return pendingRequests.Select(b => new ActiveBorrowingResponse
        {
            BorrowingRecordId = b.Id,
            LabAssetId = b.LabAssetId,
            AssetName = b.LabAsset.Name,
            SerialNumber = b.LabAsset.SerialNumber,
            RequestedStartDate = b.RequestedStartDate,
            RequestedEndDate = b.RequestedEndDate,
            Status = b.Status,
            UserName = b.User.FullName
        });
    }
}