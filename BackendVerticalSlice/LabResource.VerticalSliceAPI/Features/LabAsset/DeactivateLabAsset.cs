using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class DeactivateLabAsset
{
    public record Command(Guid Id) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var asset = await _context.LabAssets
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null)
            {
                return false;
            }

            asset.IsActive = false;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}