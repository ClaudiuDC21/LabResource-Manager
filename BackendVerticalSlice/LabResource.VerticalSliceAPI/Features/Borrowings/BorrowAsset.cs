using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class BorrowAsset
{
    public record Command(Guid UserId, Guid LabAssetId) : IRequest<Result>;

    public record Result(Guid Id, Guid UserId, Guid LabAssetId, DateTime BorrowedAt, string AssetName, string UserName);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null || !user.IsActive)
            {
                throw new ArgumentException("User not found or inactive.");
            }

            var asset = await _context.LabAssets
                .FirstOrDefaultAsync(a => a.Id == request.LabAssetId, cancellationToken);

            if (asset == null || !asset.IsActive)
            {
                throw new ArgumentException("Asset not found or inactive.");
            }

            if (asset.Status != AssetStatus.Available)
            {
                throw new InvalidOperationException($"Asset is currently not available. Current status: {asset.Status}");
            }

            var borrowingRecord = new BorrowingRecord
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LabAssetId = asset.Id,
                BorrowedAt = DateTime.UtcNow
            };

            asset.Status = AssetStatus.Borrowed;

            await _context.BorrowingRecords.AddAsync(borrowingRecord, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(
                borrowingRecord.Id,
                user.Id,
                asset.Id,
                borrowingRecord.BorrowedAt,
                asset.Name,
                user.FullName
            );
        }
    }
}