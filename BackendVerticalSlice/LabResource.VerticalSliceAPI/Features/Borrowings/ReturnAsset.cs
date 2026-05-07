using System.Text.Json.Serialization;
using FluentValidation;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class ReturnAsset
{
    public record Command(
        [property: JsonIgnore] Guid BorrowingId,
        string? Remarks,
        bool IsDefective) : IRequest<Result>;

    public record Result(Guid BorrowingRecordId, string AssetName, DateTime ReturnedAt, AssetStatus NewStatus);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var record = await _context.BorrowingRecords.FirstOrDefaultAsync(b => b.Id == request.BorrowingId, cancellationToken);
            if (record == null) throw new NotFoundException("BorrowingRecord", request.BorrowingId);
            if (record.Status != BorrowingStatus.Active) throw new ConflictException("Borrowing is not active.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == record.LabAssetId, cancellationToken);

            record.Status = BorrowingStatus.Returned;
            record.ActualReturnedAt = DateTime.UtcNow;
            record.Remarks = string.IsNullOrEmpty(record.Remarks) ? request.Remarks : $"{record.Remarks} | Return Note: {request.Remarks}";

            if (asset != null) asset.Status = request.IsDefective ? AssetStatus.Defective : AssetStatus.Available;

            await _context.SaveChangesAsync(cancellationToken);

            return new Result(record.Id, asset?.Name ?? "Unknown", record.ActualReturnedAt.Value, asset?.Status ?? AssetStatus.Available);
        }
    }
}