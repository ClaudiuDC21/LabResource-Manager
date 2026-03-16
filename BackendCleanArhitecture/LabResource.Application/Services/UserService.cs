using LabResource.Application.DTOs;
using LabResource.Application.DTOs.Users;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Interfaces.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;

namespace LabResource.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> LoginOrRegisterAsync(LoginOrRegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return MapToResponse(existingUser);
        }

        var role = DetermineUserRole(request.Email);
        var newUser = await RegisterNewUserAsync(request.FullName, request.Email, role);

        return MapToResponse(newUser);
    }

    private UserRole DetermineUserRole(string email)
    {
        if (email.EndsWith("@stud.ubbcluj.ro", StringComparison.OrdinalIgnoreCase))
            return UserRole.Student;

        if (email.EndsWith("@ubbcluj.ro", StringComparison.OrdinalIgnoreCase))
            return UserRole.Teacher;

        throw new ArgumentException("The email address must belong to the UBB domain (@stud.ubbcluj.ro or @ubbcluj.ro).");
    }

    private async Task<User> RegisterNewUserAsync(string fullName, string email, UserRole role)
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

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return newUser;
    }

    private UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}