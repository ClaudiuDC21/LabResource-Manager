using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Users;

public static class GetAllActiveUsers
{
    public record Query() : IRequest<IEnumerable<Result>>;

    public record Result(Guid Id, string FullName, string Email, UserRole Role);

    public class Handler : IRequestHandler<Query, IEnumerable<Result>>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new Result(u.Id, u.FullName, u.Email, u.Role))
                .ToListAsync(cancellationToken);
        }
    }
}