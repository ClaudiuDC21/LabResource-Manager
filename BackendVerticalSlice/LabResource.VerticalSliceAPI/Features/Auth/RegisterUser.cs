using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Auth;

public static class RegisterUser
{
    public record Command(string FullName, string Email, string? MatriculationNumber, string Password) : IRequest<Result>;

    public record Result(Guid Id, string FullName, string Email, UserRole Role, bool IsActive);

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            {
                throw new ArgumentException("Email is already in use.");
            }

            var assignedRole = request.Email.EndsWith("@ubbcluj.ro", StringComparison.OrdinalIgnoreCase)
                ? UserRole.Teacher
                : UserRole.Student;

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                MatriculationNumber = request.MatriculationNumber,
                Role = assignedRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Result(
                newUser.Id,
                newUser.FullName,
                newUser.Email,
                newUser.Role,
                newUser.IsActive
            );
        }
    }
}