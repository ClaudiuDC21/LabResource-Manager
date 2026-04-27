using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class RequestAsset
{
    public record Command(
        [property: JsonRequired] Guid UserId,
        [property: JsonRequired] Guid LabAssetId,
        DateTime RequestedStartDate,
        DateTime RequestedEndDate) : IRequest<Result>;

    public record Result(
        Guid Id,
        Guid UserId,
        Guid LabAssetId,
        string AssetName,
        string UserName,
        DateTime RequestedStartDate,
        DateTime RequestedEndDate,
        BorrowingStatus Status);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || !user.IsActive) throw new ArgumentException("User not found or inactive.");

            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == request.LabAssetId, cancellationToken);
            if (asset == null || !asset.IsActive) throw new ArgumentException("Asset not found or inactive.");

            bool hasOverlap = await _context.BorrowingRecords.AnyAsync(b =>
                b.LabAssetId == asset.Id &&
                (b.Status == BorrowingStatus.Pending || b.Status == BorrowingStatus.Approved || b.Status == BorrowingStatus.Active) &&
                b.RequestedStartDate < request.RequestedEndDate &&
                b.RequestedEndDate > request.RequestedStartDate, cancellationToken);

            if (hasOverlap) throw new InvalidOperationException("The asset is already booked for the requested period.");

            bool isAssignedTeacher = asset.AssignedTeacherId == user.Id;

            var borrowingRecord = new BorrowingRecord
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LabAssetId = asset.Id,
                RequestedStartDate = request.RequestedStartDate,
                RequestedEndDate = request.RequestedEndDate,
                Status = isAssignedTeacher ? BorrowingStatus.Approved : BorrowingStatus.Pending
            };

            asset.Status = isAssignedTeacher ? AssetStatus.Borrowed : AssetStatus.PendingApproval;

            await _context.BorrowingRecords.AddAsync(borrowingRecord, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(
                borrowingRecord.Id,
                user.Id,
                asset.Id,
                asset.Name,
                user.FullName,
                borrowingRecord.RequestedStartDate,
                borrowingRecord.RequestedEndDate,
                borrowingRecord.Status
            );
        }
    }
}