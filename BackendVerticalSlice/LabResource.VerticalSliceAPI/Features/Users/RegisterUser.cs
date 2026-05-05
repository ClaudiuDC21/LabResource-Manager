using FluentValidation;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Features.Users;

public static class RegisterUser
{
    public record Command(string FullName, string Email, string? MatriculationNumber, string Password) : IRequest<Result>;

    public record Result(Guid Id, string FullName, string Email, UserRole Role, bool IsActive);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character (e.g., ! . ? *).");

            RuleFor(x => x.MatriculationNumber)
                .MaximumLength(20).WithMessage("Matriculation number cannot exceed 20 characters.")
                .When(x => !string.IsNullOrEmpty(x.MatriculationNumber));
        }
    }

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
                throw new AlreadyExistsException("User", request.Email);
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