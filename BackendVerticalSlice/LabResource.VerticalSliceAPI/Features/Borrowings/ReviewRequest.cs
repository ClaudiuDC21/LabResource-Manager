using System.Text.Json.Serialization;
using FluentValidation;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class ReviewRequest
{
    public record Command(
        [property: JsonIgnore] Guid BorrowingId,
        [property: JsonIgnore] Guid TeacherId,
        bool IsApproved,
        string? TeacherNotes) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TeacherNotes).MaximumLength(500);
        }
    }

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
            if (record.Status != BorrowingStatus.Pending) throw new ConflictException("Only pending requests can be reviewed.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == record.LabAssetId, cancellationToken);
            if (asset != null && asset.AssignedTeacherId != request.TeacherId) throw new ForbiddenAccessException("Not authorized for this asset.");

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
        }
    }
}