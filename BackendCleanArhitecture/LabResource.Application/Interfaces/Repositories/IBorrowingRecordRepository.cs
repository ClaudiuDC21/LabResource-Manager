using LabResource.Domain.Entities;

namespace LabResource.Application.Interfaces.Repositories;

public interface IBorrowingRecordRepository
{
    Task AddAsync(BorrowingRecord record);
    Task<BorrowingRecord?> GetByIdAsync(Guid id);
    Task<BorrowingRecord?> GetActiveBorrowingByAssetIdAsync(Guid labAssetId);
    Task<IEnumerable<BorrowingRecord>> GetActiveBorrowingsByUserIdAsync(Guid userId);
    Task<IEnumerable<BorrowingRecord>> GetHistoryByAssetIdAsync(Guid labAssetId);
    Task<IEnumerable<BorrowingRecord>> GetHistoryByUserIdAsync(Guid userId);
    Task<bool> HasOverlappingReservationsAsync(Guid labAssetId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<BorrowingRecord>> GetPendingRequestsForTeacherAsync(Guid teacherId);
    Task UpdateAsync(BorrowingRecord record);
    Task SaveChangesAsync();
}