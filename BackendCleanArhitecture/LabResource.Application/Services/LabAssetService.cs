using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Interfaces.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;

namespace LabResource.Application.Services;

public class LabAssetService : ILabAssetService
{
    private readonly ILabAssetRepository _labAssetRepository;

    public LabAssetService(ILabAssetRepository labAssetRepository)
    {
        _labAssetRepository = labAssetRepository;
    }

    public async Task<LabAssetResponse> CreateAssetAsync(CreateLabAssetRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            var existingAsset = await _labAssetRepository.GetBySerialNumberAsync(request.SerialNumber);
            if (existingAsset != null)
            {
                throw new ArgumentException($"An asset with serial number '{request.SerialNumber}' already exists.");
            }
        }

        var newAsset = new LabAsset
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SerialNumber = request.SerialNumber,
            Status = AssetStatus.Available,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _labAssetRepository.AddAsync(newAsset);
        await _labAssetRepository.SaveChangesAsync();

        return MapToResponse(newAsset);
    }

    public async Task<IEnumerable<LabAssetResponse>> GetAllActiveAssetsAsync()
    {
        var assets = await _labAssetRepository.GetAllActiveAsync();
        return assets.Select(MapToResponse);
    }

    public async Task<LabAssetResponse?> GetAssetByIdAsync(Guid id)
    {
        var asset = await _labAssetRepository.GetByIdAsync(id);

        return asset != null ? MapToResponse(asset) : null;
    }

    public async Task<bool> UpdateAssetAsync(Guid id, CreateLabAssetRequest request)
    {
        var asset = await _labAssetRepository.GetByIdAsync(id);
        if (asset == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.SerialNumber) && request.SerialNumber != asset.SerialNumber)
        {
            var existingAsset = await _labAssetRepository.GetBySerialNumberAsync(request.SerialNumber);
            if (existingAsset != null)
            {
                throw new ArgumentException($"An asset with serial number '{request.SerialNumber}' already exists.");
            }
        }

        asset.Name = request.Name;
        asset.SerialNumber = request.SerialNumber;

        await _labAssetRepository.UpdateAsync(asset);
        await _labAssetRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAssetAsync(Guid id)
    {
        var asset = await _labAssetRepository.GetByIdAsync(id);
        if (asset == null)
        {
            return false;
        }

        asset.IsActive = false;

        await _labAssetRepository.UpdateAsync(asset);
        await _labAssetRepository.SaveChangesAsync();

        return true;
    }

    private LabAssetResponse MapToResponse(LabAsset asset)
    {
        return new LabAssetResponse
        {
            Id = asset.Id,
            Name = asset.Name,
            SerialNumber = asset.SerialNumber,
            Status = asset.Status,
            IsActive = asset.IsActive
        };
    }
}