using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Users;

public static class GetUserById
{
    public record Query(Guid Id) : IRequest<Result?>;

    public record Result(Guid Id, string FullName, string Email, UserRole Role);

    public class Handler : IRequestHandler<Query, Result?>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result?> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                return null;
            }

            return new Result(user.Id, user.FullName, user.Email, user.Role);
        }
    }
}