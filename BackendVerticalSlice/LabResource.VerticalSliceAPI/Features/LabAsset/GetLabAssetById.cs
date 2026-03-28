using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class GetLabAssetById
{
    public record Query(Guid Id) : IRequest<Result?>;

    public record Result(Guid Id, string Name, string? SerialNumber, AssetStatus Status, bool IsActive);

    public class Handler : IRequestHandler<Query, Result?>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result?> Handle(Query request, CancellationToken cancellationToken)
        {
            var asset = await _context.LabAssets
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null)
            {
                return null;
            }

            return new Result(asset.Id, asset.Name, asset.SerialNumber, asset.Status, asset.IsActive);
        }
    }
}