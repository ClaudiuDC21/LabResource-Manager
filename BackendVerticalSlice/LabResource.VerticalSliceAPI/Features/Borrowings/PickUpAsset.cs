using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class PickUpAsset
{
    public record Command(Guid BorrowingId) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var record = await _context.BorrowingRecords.FirstOrDefaultAsync(b => b.Id == request.BorrowingId, cancellationToken);
            if (record == null) throw new NotFoundException("BorrowingRecord", request.BorrowingId);
            if (record.Status != BorrowingStatus.Approved) throw new ConflictException("Reservation is not approved.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == record.LabAssetId, cancellationToken);
            if (asset != null && asset.Status != AssetStatus.Available) throw new ConflictException("Asset is not currently available.");

            record.Status = BorrowingStatus.Active;
            record.ActualBorrowedAt = DateTime.UtcNow;

            if (asset != null) asset.Status = AssetStatus.Borrowed;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}