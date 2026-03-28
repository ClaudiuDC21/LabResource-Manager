using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class ReturnAsset
{
    public record Command(Guid LabAssetId, string? Remarks, bool IsDefective) : IRequest<Result>;

    public record Result(Guid BorrowingRecordId, string AssetName, DateTime ReturnedAt, AssetStatus NewStatus);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var activeBorrowing = await _context.BorrowingRecords
                .FirstOrDefaultAsync(b => b.LabAssetId == request.LabAssetId && b.ReturnedAt == null, cancellationToken);

            if (activeBorrowing == null)
            {
                throw new InvalidOperationException("No active borrowing record found for this asset.");
            }

            var asset = await _context.LabAssets
                .FirstOrDefaultAsync(a => a.Id == request.LabAssetId, cancellationToken);

            if (asset == null)
            {
                throw new ArgumentException("Asset not found.");
            }

            activeBorrowing.ReturnedAt = DateTime.UtcNow;
            activeBorrowing.Remarks = request.Remarks;

            asset.Status = request.IsDefective ? AssetStatus.Defective : AssetStatus.Available;

            await _context.SaveChangesAsync(cancellationToken);

            return new Result(
                activeBorrowing.Id,
                asset.Name,
                activeBorrowing.ReturnedAt.Value,
                asset.Status
            );
        }
    }
}