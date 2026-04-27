using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class UpdateLabAsset
{
    public record Command(
        [property: JsonRequired] Guid Id,
        string Name,
        string? SerialNumber,
        string? Location,
        Guid? AssignedTeacherId) : IRequest<bool>;

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

            if (!string.IsNullOrWhiteSpace(request.SerialNumber) && request.SerialNumber != asset.SerialNumber)
            {
                var existingAsset = await _context.LabAssets
                    .FirstOrDefaultAsync(a => a.SerialNumber == request.SerialNumber, cancellationToken);

                if (existingAsset != null)
                {
                    throw new ArgumentException($"An asset with serial number '{request.SerialNumber}' already exists.");
                }
            }

            asset.Name = request.Name;
            asset.SerialNumber = request.SerialNumber;
            asset.Location = request.Location;
            asset.AssignedTeacherId = request.AssignedTeacherId;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}