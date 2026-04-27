using LabResource.Application.DTOs.Borrowings;

namespace LabResource.Application.Interfaces.Services;

public interface IBorrowingService
{
    Task<BorrowingResponse> RequestAssetAsync(BorrowAssetRequest request);
    Task ReviewRequestAsync(Guid borrowingId, ReviewBorrowingRequest request);
    Task PickUpAssetAsync(Guid borrowingId);
    Task<ReturnAssetResponse> ReturnAssetAsync(Guid borrowingId, ReturnAssetRequest request);
    Task<IEnumerable<ActiveBorrowingResponse>> GetActiveBorrowingsForUserAsync(Guid userId);
    Task<IEnumerable<AssetHistoryResponse>> GetAssetHistoryAsync(Guid labAssetId);
    Task<IEnumerable<UserBorrowingHistoryResponse>> GetUserHistoryAsync(Guid userId);
    Task<IEnumerable<ActiveBorrowingResponse>> GetPendingRequestsForTeacherAsync(Guid teacherId);
}