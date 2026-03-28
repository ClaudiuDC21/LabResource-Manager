using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Borrowings;

public static class GetActiveBorrowingsForUser
{
    public record Query(Guid UserId) : IRequest<IEnumerable<Result>>;

    public record Result(Guid BorrowingRecordId, Guid LabAssetId, string AssetName, string? SerialNumber, DateTime BorrowedAt);

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
            if (!userExists)
            {
                throw new ArgumentException("User not found.");
            }

            return await _context.BorrowingRecords
                .Include(b => b.LabAsset)
                .Where(b => b.UserId == request.UserId && b.ReturnedAt == null)
                .Select(b => new Result(
                    b.Id,
                    b.LabAssetId,
                    b.LabAsset.Name,
                    b.LabAsset.SerialNumber,
                    b.BorrowedAt
                ))
                .ToListAsync(cancellationToken);
        }
    }
}