using LabResource.Application.DTOs.Borrowings;

namespace LabResource.Application.Interfaces.Services;

public interface IBorrowingService
{
    Task<BorrowingResponse> BorrowAssetAsync(BorrowAssetRequest request);
    Task<ReturnAssetResponse> ReturnAssetAsync(ReturnAssetRequest request);
    Task<IEnumerable<ActiveBorrowingResponse>> GetActiveBorrowingsForUserAsync(Guid userId);
    Task<IEnumerable<AssetHistoryResponse>> GetAssetHistoryAsync(Guid labAssetId);
}