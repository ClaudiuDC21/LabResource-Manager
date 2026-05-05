using FluentValidation;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.LabAssets;

public static class UpdateLabAsset
{
    public record Command([property: JsonRequired] Guid Id, string Name, string? SerialNumber, string? Location, Guid? AssignedTeacherId) : IRequest;

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

            if (request.AssignedTeacherId.HasValue)
            {
                var teacher = await _context.Users.FindAsync(new object[] { request.AssignedTeacherId.Value }, cancellationToken);
                if (teacher == null) throw new NotFoundException("User", request.AssignedTeacherId.Value);
                if (teacher.Role != UserRole.Teacher) throw new BadRequestException("Assigned user must be a Teacher.");
            }

            if (!string.IsNullOrWhiteSpace(request.SerialNumber) && request.SerialNumber != asset.SerialNumber)
            {
                if (await _context.LabAssets.AnyAsync(a => a.SerialNumber == request.SerialNumber, cancellationToken))
                {
                    throw new AlreadyExistsException("LabAsset", request.SerialNumber);
                }
            }

            asset.Name = request.Name;
            asset.SerialNumber = request.SerialNumber;
            asset.Location = request.Location;
            asset.AssignedTeacherId = request.AssignedTeacherId;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}