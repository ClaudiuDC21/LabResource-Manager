using LabResource.Application.Interfaces.Repositories;
using LabResource.Domain.Entities;
using LabResource.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabResource.Infrastructure.Repositories;

public class LabAssetRepository : ILabAssetRepository
{
    private readonly ApplicationDbContext _context;

    public LabAssetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LabAsset?> GetByIdAsync(Guid id)
    {
        return await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<LabAsset?> GetBySerialNumberAsync(string serialNumber)
    {
        return await _context.LabAssets.FirstOrDefaultAsync(a => a.SerialNumber == serialNumber);
    }

    public async Task<IEnumerable<LabAsset>> GetAllActiveAsync()
    {
        return await _context.LabAssets
            .Where(a => a.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(LabAsset labAsset)
    {
        await _context.LabAssets.AddAsync(labAsset);
    }

    public Task UpdateAsync(LabAsset labAsset)
    {
        _context.LabAssets.Update(labAsset);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}