using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class GetPendingRequestsForTeacher
{
    public record Query(Guid TeacherId) : IRequest<IEnumerable<Result>>;

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
            return await _context.BorrowingRecords
                .Include(b => b.LabAsset)
                .Include(b => b.User)
                .Where(b => b.LabAsset.AssignedTeacherId == request.TeacherId && b.Status == BorrowingStatus.Pending)
                .OrderBy(b => b.RequestedStartDate)
                .Select(b => new Result(b.Id, b.LabAssetId, b.LabAsset.Name, b.LabAsset.SerialNumber, b.User.FullName, b.RequestedStartDate, b.RequestedEndDate, b.Status))
                .ToListAsync(cancellationToken);
        }
    }
}