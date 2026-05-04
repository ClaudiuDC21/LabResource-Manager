using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class ReviewRequest
{
    public record Command( Guid BorrowingId,
        bool IsApproved,
        string? TeacherNotes) : IRequest<bool>;

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
            if (record == null) throw new ArgumentException("Borrowing record not found.");
            if (record.Status != BorrowingStatus.Pending) throw new InvalidOperationException("Only pending requests can be reviewed.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == record.LabAssetId, cancellationToken);

            if (request.IsApproved)
            {
                record.Status = BorrowingStatus.Approved;
                record.Remarks = request.TeacherNotes;
            }
            else
            {
                record.Status = BorrowingStatus.Rejected;
                record.Remarks = request.TeacherNotes;
                if (asset != null) asset.Status = AssetStatus.Available;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}