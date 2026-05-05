using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class DeactivateLabAsset
{
    public record Command(Guid Id) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var asset = await _context.LabAssets.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
            if (asset == null) throw new NotFoundException("LabAsset", request.Id);

            if (!asset.IsActive) throw new ConflictException("Asset is already deactivated.");
            if (asset.Status == AssetStatus.Borrowed) throw new ConflictException("Cannot deactivate a borrowed asset.");

            asset.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}