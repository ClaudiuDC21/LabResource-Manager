using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Interfaces.Services;
using LabResource.Application.Mappings;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Domain.Exceptions;

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
        if (user == null || !user.IsActive)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var asset = await _assetRepository.GetByIdAsync(request.LabAssetId);
        if (asset == null || !asset.IsActive)
        {
            throw new NotFoundException("LabAsset", request.LabAssetId);
        }

        if (asset.Status == AssetStatus.Defective)
        {
            throw new ConflictException("Cannot request an asset that is currently marked as defective.");
        }

        bool hasOverlap = await _borrowingRepository.HasOverlappingReservationsAsync(asset.Id, request.RequestedStartDate, request.RequestedEndDate);
        if (hasOverlap)
        {
            throw new ConflictException("The asset is already booked for the requested period.");
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

        asset.Status = isAssignedTeacher ? AssetStatus.Borrowed : AssetStatus.PendingApproval;

        await _borrowingRepository.AddAsync(borrowingRecord);
        await _assetRepository.UpdateAsync(asset);
        await _borrowingRepository.SaveChangesAsync();

        return borrowingRecord.ToBorrowingResponse(user.FullName, asset.Name);
    }

    public async Task ReviewRequestAsync(Guid borrowingId, Guid teacherId, ReviewBorrowingRequest request)
    {
        var record = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (record == null)
        {
            throw new NotFoundException("BorrowingRecord", borrowingId);
        }

        if (record.Status != BorrowingStatus.Pending)
        {
            throw new ConflictException("Only pending requests can be reviewed.");
        }

        var asset = await _assetRepository.GetByIdAsync(record.LabAssetId);

        if (asset != null && asset.AssignedTeacherId != teacherId)
        {
            throw new ForbiddenAccessException("You are not authorized to review requests for this asset.");
        }

        if (request.IsApproved)
        {
            record.Status = BorrowingStatus.Approved;
            record.Remarks = request.TeacherNotes;
        }
        else
        {
            record.Status = BorrowingStatus.Rejected;
            record.Remarks = request.TeacherNotes;

            if (asset != null)
            {
                asset.Status = AssetStatus.Available;
                await _assetRepository.UpdateAsync(asset);
            }
        }

        await _borrowingRepository.UpdateAsync(record);
        await _borrowingRepository.SaveChangesAsync();
    }

    public async Task PickUpAssetAsync(Guid borrowingId)
    {
        var record = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (record == null)
        {
            throw new NotFoundException("BorrowingRecord", borrowingId);
        }

        if (record.Status != BorrowingStatus.Approved)
        {
            throw new ConflictException("Reservation must be approved before pickup.");
        }

        var asset = await _assetRepository.GetByIdAsync(record.LabAssetId);

        if (asset != null && asset.Status != AssetStatus.PendingApproval && asset.Status != AssetStatus.Borrowed)
        {
            throw new ConflictException($"Asset cannot be picked up because its current status is {asset.Status}. It must be awaiting pickup.");
        }

        record.Status = BorrowingStatus.Active;
        record.ActualBorrowedAt = DateTime.UtcNow;

        if (asset != null)
        {
            asset.Status = AssetStatus.Borrowed;
            await _assetRepository.UpdateAsync(asset);
        }

        await _borrowingRepository.UpdateAsync(record);
        await _borrowingRepository.SaveChangesAsync();
    }

    public async Task<ReturnAssetResponse> ReturnAssetAsync(Guid borrowingId, ReturnAssetRequest request)
    {
        var activeBorrowing = await _borrowingRepository.GetByIdAsync(borrowingId);
        if (activeBorrowing == null)
        {
            throw new NotFoundException("BorrowingRecord", borrowingId);
        }

        if (activeBorrowing.Status != BorrowingStatus.Active)
        {
            throw new ConflictException("Cannot return an asset that is not currently active.");
        }

        var asset = await _assetRepository.GetByIdAsync(activeBorrowing.LabAssetId);

        activeBorrowing.Status = BorrowingStatus.Returned;
        activeBorrowing.ActualReturnedAt = DateTime.UtcNow;
        activeBorrowing.Remarks = string.IsNullOrEmpty(activeBorrowing.Remarks)
            ? request.Remarks
            : $"{activeBorrowing.Remarks} | Return Note: {request.Remarks}";

        if (asset != null)
        {
            asset.Status = request.IsDefective ? AssetStatus.Defective : AssetStatus.Available;
            await _assetRepository.UpdateAsync(asset);
        }

        await _borrowingRepository.UpdateAsync(activeBorrowing);
        await _borrowingRepository.SaveChangesAsync();

        return new ReturnAssetResponse
        {
            BorrowingRecordId = activeBorrowing.Id,
            AssetName = asset?.Name ?? "Unknown",
            ReturnedAt = activeBorrowing.ActualReturnedAt.Value,
            NewStatus = asset?.Status ?? AssetStatus.Available
        };
    }

    public async Task<IEnumerable<ActiveBorrowingResponse>> GetActiveBorrowingsForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsByUserIdAsync(userId);
        return activeBorrowings.Select(b => b.ToActiveBorrowingResponse());
    }

    public async Task<IEnumerable<AssetHistoryResponse>> GetAssetHistoryAsync(Guid labAssetId)
    {
        var asset = await _assetRepository.GetByIdAsync(labAssetId);
        if (asset == null)
        {
            throw new NotFoundException("LabAsset", labAssetId);
        }

        var history = await _borrowingRepository.GetHistoryByAssetIdAsync(labAssetId);
        return history.Select(b => b.ToAssetHistoryResponse());
    }

    public async Task<IEnumerable<UserBorrowingHistoryResponse>> GetUserHistoryAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var history = await _borrowingRepository.GetHistoryByUserIdAsync(userId);
        return history.Select(b => b.ToUserBorrowingHistoryResponse());
    }

    public async Task<IEnumerable<ActiveBorrowingResponse>> GetPendingRequestsForTeacherAsync(Guid teacherId)
    {
        var pendingRequests = await _borrowingRepository.GetPendingRequestsForTeacherAsync(teacherId);
        return pendingRequests.Select(b => b.ToActiveBorrowingResponse());
    }
}