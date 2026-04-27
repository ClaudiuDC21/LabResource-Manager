using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class ReturnAsset
{
    public record Command(
        [property: JsonIgnore] Guid BorrowingId,
        string? Remarks,
        bool IsDefective) : IRequest<Result>;

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
            var activeBorrowing = await _context.BorrowingRecords.FirstOrDefaultAsync(b => b.Id == request.BorrowingId, cancellationToken);
            if (activeBorrowing == null) throw new InvalidOperationException("Borrowing record not found.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == activeBorrowing.LabAssetId, cancellationToken);

            activeBorrowing.Status = BorrowingStatus.Returned;
            activeBorrowing.ActualReturnedAt = DateTime.UtcNow;
            activeBorrowing.Remarks = string.IsNullOrEmpty(activeBorrowing.Remarks)
                ? request.Remarks
                : $"{activeBorrowing.Remarks} | Return Note: {request.Remarks}";

            if (asset != null) asset.Status = request.IsDefective ? AssetStatus.Defective : AssetStatus.Available;

            await _context.SaveChangesAsync(cancellationToken);

            return new Result(
                activeBorrowing.Id,
                asset?.Name ?? string.Empty,
                activeBorrowing.ActualReturnedAt.Value,
                asset?.Status ?? AssetStatus.Available
            );
        }
    }
}