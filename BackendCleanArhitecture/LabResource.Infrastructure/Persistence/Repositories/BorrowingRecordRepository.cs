using LabResource.Application.Interfaces.Repositories;
using LabResource.Domain.Entities;
using LabResource.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabResource.Infrastructure.Repositories;

public class BorrowingRecordRepository : IBorrowingRecordRepository
{
    private readonly ApplicationDbContext _context;

    public BorrowingRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BorrowingRecord record)
    {
        await _context.BorrowingRecords.AddAsync(record);
    }

    public async Task<BorrowingRecord?> GetActiveBorrowingByAssetIdAsync(Guid labAssetId)
    {
        return await _context.BorrowingRecords
            .FirstOrDefaultAsync(b => b.LabAssetId == labAssetId && b.ReturnedAt == null);
    }

    public async Task<IEnumerable<BorrowingRecord>> GetActiveBorrowingsByUserIdAsync(Guid userId)
    {
        return await _context.BorrowingRecords
            .Include(b => b.LabAsset)
            .Where(b => b.UserId == userId && b.ReturnedAt == null)
            .ToListAsync();
    }

    public async Task<IEnumerable<BorrowingRecord>> GetHistoryByAssetIdAsync(Guid labAssetId)
    {
        return await _context.BorrowingRecords
            .Include(b => b.User)
            .Where(b => b.LabAssetId == labAssetId)
            .OrderByDescending(b => b.BorrowedAt)
            .ToListAsync();
    }

    public Task UpdateAsync(BorrowingRecord record)
    {
        _context.BorrowingRecords.Update(record);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}