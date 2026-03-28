using LabResource.Domain.Entities;

namespace LabResource.Application.Interfaces.Repositories;

public interface IBorrowingRecordRepository
{
    Task AddAsync(BorrowingRecord record);
    Task<BorrowingRecord?> GetActiveBorrowingByAssetIdAsync(Guid labAssetId);
    Task UpdateAsync(BorrowingRecord record);
    Task SaveChangesAsync();
}