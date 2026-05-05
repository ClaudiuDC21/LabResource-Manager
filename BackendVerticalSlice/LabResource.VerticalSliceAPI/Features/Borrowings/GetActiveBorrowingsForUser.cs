using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class GetActiveBorrowingsForUser
{
    public record Query(Guid UserId) : IRequest<IEnumerable<Result>>;

    public record Result(Guid BorrowingRecordId, Guid LabAssetId, string AssetName, string? SerialNumber, string? UserName, DateTime RequestedStartDate, DateTime RequestedEndDate, BorrowingStatus Status);

    public class Handler : IRequestHandler<Query, IEnumerable<Result>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
                throw new NotFoundException("User", request.UserId);

            return await _context.BorrowingRecords
                .Include(b => b.LabAsset)
                .Include(b => b.User)
                .Where(b => b.UserId == request.UserId && b.ActualReturnedAt == null && (b.Status == BorrowingStatus.Active || b.Status == BorrowingStatus.Approved))
                .Select(b => new Result(b.Id, b.LabAssetId, b.LabAsset.Name, b.LabAsset.SerialNumber, b.User.FullName, b.RequestedStartDate, b.RequestedEndDate, b.Status))
                .ToListAsync(cancellationToken);
        }
    }
}