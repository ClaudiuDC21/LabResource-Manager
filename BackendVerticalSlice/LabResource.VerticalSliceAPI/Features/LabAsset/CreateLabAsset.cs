using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class CreateLabAsset
{
    public record Command(string Name, string? SerialNumber) : IRequest<Result>;

    public record Result(Guid Id, string Name, string? SerialNumber, AssetStatus Status, bool IsActive);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                var existingAsset = await _context.LabAssets
                    .FirstOrDefaultAsync(a => a.SerialNumber == request.SerialNumber, cancellationToken);

                if (existingAsset != null)
                {
                    throw new ArgumentException($"An asset with serial number '{request.SerialNumber}' already exists.");
                }
            }

            var newAsset = new LabAsset
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                SerialNumber = request.SerialNumber,
                Status = AssetStatus.Available,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.LabAssets.AddAsync(newAsset, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(newAsset.Id, newAsset.Name, newAsset.SerialNumber, newAsset.Status, newAsset.IsActive);
        }
    }
}