using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Users;

public static class LoginOrRegisterUser
{
    public record Command(string FullName, string Email) : IRequest<Result>;

    public record Result(Guid Id, string FullName, string Email, UserRole Role);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return MapToResult(existingUser);
            }

            var role = DetermineUserRole(request.Email);
            var newUser = await RegisterNewUserAsync(request.FullName, request.Email, role, cancellationToken);

            return MapToResult(newUser);
        }

        private UserRole DetermineUserRole(string email)
        {
            if (email.EndsWith("@stud.ubbcluj.ro", StringComparison.OrdinalIgnoreCase))
                return UserRole.Student;

            if (email.EndsWith("@ubbcluj.ro", StringComparison.OrdinalIgnoreCase))
                return UserRole.Teacher;

            throw new ArgumentException("The email address must belong to the UBB domain (@stud.ubbcluj.ro or @ubbcluj.ro).");
        }

        private async Task<User> RegisterNewUserAsync(string fullName, string email, UserRole role, CancellationToken cancellationToken)
        {
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return newUser;
        }

        private static Result MapToResult(User user)
        {
            return new Result(user.Id, user.FullName, user.Email, user.Role);
        }
    }
}