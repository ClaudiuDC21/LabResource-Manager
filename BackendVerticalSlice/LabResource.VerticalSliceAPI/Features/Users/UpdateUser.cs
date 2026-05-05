using FluentValidation;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.Users;

public static class UpdateUser
{
    public record Command([property: JsonRequired] Guid Id, string FullName, string? MatriculationNumber) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.MatriculationNumber)
                .MaximumLength(20).WithMessage("Matriculation number cannot exceed 20 characters.")
                .When(x => !string.IsNullOrEmpty(x.MatriculationNumber));
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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("User", request.Id);
            }

            user.FullName = request.FullName;
            user.MatriculationNumber = request.MatriculationNumber;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}