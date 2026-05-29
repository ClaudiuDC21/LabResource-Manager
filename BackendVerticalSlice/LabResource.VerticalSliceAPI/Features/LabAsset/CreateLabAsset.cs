using FluentValidation;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class CreateLabAsset
{
    public record Command(string Name, string? SerialNumber, string? Location, Guid? AssignedTeacherId) : IRequest<Result>;

    public record Result(Guid Id, string Name, string? SerialNumber, string? Location, Guid? AssignedTeacherId, AssetStatus Status, bool IsActive);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Asset name is required.")
                .MaximumLength(150).WithMessage("Asset name cannot exceed 150 characters.");

            RuleFor(x => x.SerialNumber)
                .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.SerialNumber));

            RuleFor(x => x.Location)
                .MaximumLength(100).WithMessage("Location cannot exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Location));
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            await LabAssetBusinessRules.ValidateTeacherAsync(_context, request.AssignedTeacherId, cancellationToken);
            await LabAssetBusinessRules.ValidateSerialNumberUniquenessAsync(_context, request.SerialNumber, null, cancellationToken);

            var newAsset = new LabAsset
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                SerialNumber = request.SerialNumber,
                Location = request.Location,
                AssignedTeacherId = request.AssignedTeacherId,
                Status = AssetStatus.Available,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.LabAssets.AddAsync(newAsset, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Result(newAsset.Id, newAsset.Name, newAsset.SerialNumber, newAsset.Location, newAsset.AssignedTeacherId, newAsset.Status, newAsset.IsActive);
        }
    }
}