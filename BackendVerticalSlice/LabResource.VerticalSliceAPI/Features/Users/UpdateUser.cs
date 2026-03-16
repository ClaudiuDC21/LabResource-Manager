using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace LabResource.VerticalApi.Features.Users;

public static class UpdateUser
{
    public record Command([property: JsonRequired] Guid Id, string FullName, string? MatriculationNumber) : IRequest<bool>;

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                return false;
            }

            user.FullName = request.FullName;
            user.MatriculationNumber = request.MatriculationNumber;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}