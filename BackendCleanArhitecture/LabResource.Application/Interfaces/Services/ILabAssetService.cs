using LabResource.Application.DTOs.LabAssets;

namespace LabResource.Application.Interfaces.Services;

public interface ILabAssetService
{
    Task<LabAssetResponse> CreateAssetAsync(CreateLabAssetRequest request);
    Task<IEnumerable<LabAssetResponse>> GetAllActiveAssetsAsync();
    Task<LabAssetResponse?> GetAssetByIdAsync(Guid id);
    Task<bool> UpdateAssetAsync(Guid id, CreateLabAssetRequest request);
    Task<bool> DeactivateAssetAsync(Guid id);
}