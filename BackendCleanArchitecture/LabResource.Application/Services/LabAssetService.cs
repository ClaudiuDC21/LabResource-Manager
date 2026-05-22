using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Interfaces.Services;
using LabResource.Application.Mappings;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Domain.Exceptions;

namespace LabResource.Application.Services;

public class LabAssetService : ILabAssetService
{
    private readonly ILabAssetRepository _labAssetRepository;
    private readonly IUserRepository _userRepository;

    public LabAssetService(ILabAssetRepository labAssetRepository, IUserRepository userRepository)
    {
        _labAssetRepository = labAssetRepository;
        _userRepository = userRepository;
    }

    public async Task<LabAssetResponse> CreateAssetAsync(CreateLabAssetRequest request)
    {
        await ValidateTeacherAsync(request.AssignedTeacherId);

        if (!string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            var existingAsset = await _labAssetRepository.GetBySerialNumberAsync(request.SerialNumber);
            if (existingAsset != null)
            {
                throw new AlreadyExistsException("LabAsset", request.SerialNumber);
            }
        }

        var newAsset = new LabAsset
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SerialNumber = request.SerialNumber,
            Location = request.Location,
            AssignedTeacherId = request.AssignedTeacherId,
            Status = AssetStatus.Available,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _labAssetRepository.AddAsync(newAsset);
        await _labAssetRepository.SaveChangesAsync();

        return newAsset.ToResponse();
    }

    public async Task<IEnumerable<LabAssetResponse>> GetAllActiveAssetsAsync()
    {
        var assets = await _labAssetRepository.GetAllActiveAsync();
        return assets.Select(asset => asset.ToResponse());
    }

    public async Task<LabAssetResponse> GetAssetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided asset ID is invalid.");
        }

        var asset = await _labAssetRepository.GetByIdAsync(id);

        if (asset == null)
        {
            throw new NotFoundException("LabAsset", id);
        }

        return asset.ToResponse();
    }

    public async Task<bool> UpdateAssetAsync(Guid id, UpdateLabAssetRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided asset ID is invalid.");
        }

        var asset = await _labAssetRepository.GetByIdAsync(id);

        if (asset == null)
        {
            throw new NotFoundException("LabAsset", id);
        }

        await ValidateTeacherAsync(request.AssignedTeacherId);

        if (!string.IsNullOrWhiteSpace(request.SerialNumber) && request.SerialNumber != asset.SerialNumber)
        {
            var existingAsset = await _labAssetRepository.GetBySerialNumberAsync(request.SerialNumber);
            if (existingAsset != null)
            {
                throw new AlreadyExistsException("LabAsset", request.SerialNumber);
            }
        }

        asset.Name = request.Name;
        asset.SerialNumber = request.SerialNumber;
        asset.Location = request.Location;
        asset.AssignedTeacherId = request.AssignedTeacherId;

        if (request.IsDefective)
        {
            asset.Status = AssetStatus.Defective;
        }
        else if (asset.Status == AssetStatus.Defective)
        {
            asset.Status = AssetStatus.Available;
        }

        await _labAssetRepository.UpdateAsync(asset);
        await _labAssetRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAssetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided asset ID is invalid.");
        }

        var asset = await _labAssetRepository.GetByIdAsync(id);

        if (asset == null)
        {
            throw new NotFoundException("LabAsset", id);
        }

        if (!asset.IsActive)
        {
            throw new ConflictException("This asset is already deactivated.");
        }

        if (asset.Status == AssetStatus.Borrowed)
        {
            throw new ConflictException("Cannot deactivate an asset that is currently borrowed.");
        }

        asset.IsActive = false;
        await _labAssetRepository.UpdateAsync(asset);
        await _labAssetRepository.SaveChangesAsync();

        return true;
    }

    private async Task ValidateTeacherAsync(Guid? teacherId)
    {
        if (teacherId.HasValue && teacherId.Value != Guid.Empty)
        {
            var teacher = await _userRepository.GetByIdAsync(teacherId.Value);

            if (teacher == null)
            {
                throw new NotFoundException("User", teacherId.Value);
            }

            if (teacher.Role != UserRole.Teacher)
            {
                throw new BadRequestException("The assigned user must have the Teacher role.");
            }
        }
    }
}