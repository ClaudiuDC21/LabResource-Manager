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

    public async Task<BorrowingResponse> BorrowAssetAsync(BorrowAssetRequest request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || !user.IsActive)
        {
            throw new ArgumentException("User not found or inactive.");
        }

        var asset = await _assetRepository.GetByIdAsync(request.LabAssetId);
        if (asset == null || !asset.IsActive)
        {
            throw new ArgumentException("Asset not found or inactive.");
        }

        if (asset.Status != AssetStatus.Available)
        {
            throw new InvalidOperationException($"Asset is currently not available. Current status: {asset.Status}");
        }

        var borrowingRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LabAssetId = asset.Id,
            BorrowedAt = DateTime.UtcNow
        };

        asset.Status = AssetStatus.Borrowed;

        await _borrowingRepository.AddAsync(borrowingRecord);
        await _assetRepository.UpdateAsync(asset);

        await _borrowingRepository.SaveChangesAsync();

        return new BorrowingResponse
        {
            Id = borrowingRecord.Id,
            UserId = user.Id,
            LabAssetId = asset.Id,
            BorrowedAt = borrowingRecord.BorrowedAt,
            AssetName = asset.Name,
            UserName = user.FullName
        };
    }

    public async Task<ReturnAssetResponse> ReturnAssetAsync(ReturnAssetRequest request)
    {
        var activeBorrowing = await _borrowingRepository.GetActiveBorrowingByAssetIdAsync(request.LabAssetId);
        if (activeBorrowing == null)
        {
            throw new InvalidOperationException("No active borrowing record found for this asset.");
        }

        var asset = await _assetRepository.GetByIdAsync(request.LabAssetId);
        if (asset == null)
        {
            throw new ArgumentException("Asset not found.");
        }

        activeBorrowing.ReturnedAt = DateTime.UtcNow;
        activeBorrowing.Remarks = request.Remarks;

        asset.Status = request.IsDefective ? AssetStatus.Defective : AssetStatus.Available;

        await _borrowingRepository.UpdateAsync(activeBorrowing);
        await _assetRepository.UpdateAsync(asset);
        await _borrowingRepository.SaveChangesAsync();

        return new ReturnAssetResponse
        {
            BorrowingRecordId = activeBorrowing.Id,
            AssetName = asset.Name,
            ReturnedAt = activeBorrowing.ReturnedAt.Value,
            NewStatus = asset.Status
        };
    }

    public async Task<IEnumerable<ActiveBorrowingResponse>> GetActiveBorrowingsForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        var activeBorrowings = await _borrowingRepository.GetActiveBorrowingsByUserIdAsync(userId);

        return activeBorrowings.Select(b => new ActiveBorrowingResponse
        {
            BorrowingRecordId = b.Id,
            LabAssetId = b.LabAssetId,
            AssetName = b.LabAsset.Name,
            SerialNumber = b.LabAsset.SerialNumber,
            BorrowedAt = b.BorrowedAt
        });
    }

    public async Task<IEnumerable<AssetHistoryResponse>> GetAssetHistoryAsync(Guid labAssetId)
    {
        var asset = await _assetRepository.GetByIdAsync(labAssetId);
        if (asset == null)
        {
            throw new ArgumentException("Asset not found.");
        }

        var history = await _borrowingRepository.GetHistoryByAssetIdAsync(labAssetId);

        return history.Select(b => new AssetHistoryResponse
        {
            BorrowingRecordId = b.Id,
            UserName = b.User.FullName,
            MatriculationNumber = b.User.MatriculationNumber,
            BorrowedAt = b.BorrowedAt,
            ReturnedAt = b.ReturnedAt,
            Remarks = b.Remarks
        });
    }
}