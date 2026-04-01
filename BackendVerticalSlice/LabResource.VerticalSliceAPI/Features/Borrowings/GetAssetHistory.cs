using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class GetAssetHistory
{
    public record Query(Guid LabAssetId) : IRequest<IEnumerable<Result>>;

    public record Result(Guid BorrowingRecordId, string UserName, string? MatriculationNumber, DateTime BorrowedAt, DateTime? ReturnedAt, string? Remarks);

    public class Handler : IRequestHandler<Query, IEnumerable<Result>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var assetExists = await _context.LabAssets.AnyAsync(a => a.Id == request.LabAssetId, cancellationToken);
            if (!assetExists)
            {
                throw new ArgumentException("Asset not found.");
            }

            return await _context.BorrowingRecords
                .Include(b => b.User)
                .Where(b => b.LabAssetId == request.LabAssetId)
                .OrderByDescending(b => b.BorrowedAt)
                .Select(b => new Result(
                    b.Id,
                    b.User.FullName,
                    b.User.MatriculationNumber,
                    b.BorrowedAt,
                    b.ReturnedAt,
                    b.Remarks
                ))
                .ToListAsync(cancellationToken);
        }
    }
}