using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class GetAllActiveLabAssets
{
    public record Query() : IRequest<IEnumerable<Result>>;

    public record Result(Guid Id, string Name, string? SerialNumber, AssetStatus Status, bool IsActive);

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
                .Select(a => new Result(a.Id, a.Name, a.SerialNumber, a.Status, a.IsActive))
                .ToListAsync(cancellationToken);
        }
    }
}