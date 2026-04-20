using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class GetAllActiveLabAssets
{
    public record Query() : IRequest<IEnumerable<Result>>;

    // Adăugăm CurrentBorrowerName în record
    public record Result(Guid Id, string Name, string? SerialNumber, AssetStatus Status, bool IsActive, string? CurrentBorrowerName, DateTime? CurrentBorrowDate);

    public class Handler : IRequestHandler<Query, IEnumerable<Result>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.LabAssets
                .Where(a => a.IsActive)
                .Select(a => new Result(
                    a.Id,
                    a.Name,
                    a.SerialNumber,
                    a.Status,
                    a.IsActive,
                    a.BorrowingRecords
                        .Where(b => b.ReturnedAt == null)
                        .Select(b => b.User.FullName)
                        .FirstOrDefault(),
                    // AICI: Punem (DateTime?) ca să returneze null dacă aparatul nu e împrumutat
                    a.BorrowingRecords
                        .Where(b => b.ReturnedAt == null)
                        .Select(b => (DateTime?)b.BorrowedAt)
                        .FirstOrDefault()
                ))
                .ToListAsync(cancellationToken);
        }
    }
}