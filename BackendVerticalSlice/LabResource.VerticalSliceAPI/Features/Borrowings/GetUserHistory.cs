using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class GetUserHistory
{
    public record Query(Guid UserId) : IRequest<IEnumerable<Result>>;

    public record Result(string AssetName, string? SerialNumber, DateTime RequestedStartDate, DateTime RequestedEndDate, DateTime? ActualReturnedAt, BorrowingStatus Status, bool IsDefective, string? Remarks);

    public class Handler : IRequestHandler<Query, IEnumerable<Result>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
            if (!userExists) throw new ArgumentException("User not found.");

            return await _context.BorrowingRecords
                .Include(b => b.LabAsset)
                .Where(b => b.UserId == request.UserId)
                .OrderByDescending(b => b.RequestedStartDate)
                .Select(b => new Result(
                    b.LabAsset.Name,
                    b.LabAsset.SerialNumber,
                    b.RequestedStartDate,
                    b.RequestedEndDate,
                    b.ActualReturnedAt,
                    b.Status,
                    b.LabAsset.Status == AssetStatus.Defective,
                    b.Remarks
                ))
                .ToListAsync(cancellationToken);
        }
    }
}