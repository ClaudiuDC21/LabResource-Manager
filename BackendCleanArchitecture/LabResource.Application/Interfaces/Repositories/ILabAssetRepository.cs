using LabResource.Domain.Entities;

namespace LabResource.Application.Interfaces.Repositories;

public interface ILabAssetRepository
{
    Task<LabAsset?> GetByIdAsync(Guid id);
    Task<LabAsset?> GetBySerialNumberAsync(string serialNumber);
    Task<IEnumerable<LabAsset>> GetAllActiveAsync();
    Task AddAsync(LabAsset labAsset);
    Task UpdateAsync(LabAsset labAsset);
    Task SaveChangesAsync();
}