using LabResource.Application.Interfaces.Repositories;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
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

    public async Task<BorrowingRecord?> GetByIdAsync(Guid id)
    {
        return await _context.BorrowingRecords
            .Include(b => b.LabAsset)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BorrowingRecord?> GetActiveBorrowingByAssetIdAsync(Guid labAssetId)
    {
        return await _context.BorrowingRecords
            .FirstOrDefaultAsync(b => b.LabAssetId == labAssetId && b.ActualReturnedAt == null && b.Status == BorrowingStatus.Active);
    }

    public async Task<IEnumerable<BorrowingRecord>> GetActiveBorrowingsByUserIdAsync(Guid userId)
    {
        return await _context.BorrowingRecords
            .Include(b => b.LabAsset)
            .Where(b => b.UserId == userId && b.ActualReturnedAt == null && (b.Status == BorrowingStatus.Active || b.Status == BorrowingStatus.Approved))
            .ToListAsync();
    }

    public async Task<IEnumerable<BorrowingRecord>> GetHistoryByAssetIdAsync(Guid labAssetId)
    {
        return await _context.BorrowingRecords
            .Include(b => b.User)
            .Where(b => b.LabAssetId == labAssetId)
            .OrderByDescending(b => b.RequestedStartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<BorrowingRecord>> GetHistoryByUserIdAsync(Guid userId)
    {
        return await _context.BorrowingRecords
            .Include(b => b.LabAsset)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.RequestedStartDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingReservationsAsync(Guid labAssetId, DateTime startDate, DateTime endDate)
    {
        return await _context.BorrowingRecords.AnyAsync(b =>
             b.LabAssetId == labAssetId &&
             (b.Status == BorrowingStatus.Pending || b.Status == BorrowingStatus.Approved || b.Status == BorrowingStatus.Active) &&
             b.RequestedStartDate < endDate &&
             b.RequestedEndDate > startDate);
    }

    public async Task<IEnumerable<BorrowingRecord>> GetPendingRequestsForTeacherAsync(Guid teacherId)
    {
        return await _context.BorrowingRecords
            .Include(b => b.LabAsset)
            .Include(b => b.User)
            .Where(b => b.LabAsset.AssignedTeacherId == teacherId && b.Status == BorrowingStatus.Pending)
            .OrderBy(b => b.RequestedStartDate)
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