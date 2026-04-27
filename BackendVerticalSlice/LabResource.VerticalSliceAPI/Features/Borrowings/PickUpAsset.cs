using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class PickUpAsset
{
    public record Command(Guid BorrowingId) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var record = await _context.BorrowingRecords.FirstOrDefaultAsync(b => b.Id == request.BorrowingId, cancellationToken);
            if (record == null || record.Status != BorrowingStatus.Approved)
                throw new InvalidOperationException("Reservation is not approved yet.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == record.LabAssetId, cancellationToken);

            record.Status = BorrowingStatus.Active;
            record.ActualBorrowedAt = DateTime.UtcNow;

            if (asset != null) asset.Status = AssetStatus.Borrowed;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}