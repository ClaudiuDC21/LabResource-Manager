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

    public async Task<UserResponse> RegisterUserAsync(RegisterUserRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("Email is already in use.");
        }

        var assignedRole = request.Email.EndsWith("@ubbcluj.ro", StringComparison.OrdinalIgnoreCase)
            ? Domain.Enums.UserRole.Teacher 
            : Domain.Enums.UserRole.Student; 

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

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return new UserResponse
        {
            Id = newUser.Id,
            FullName = newUser.FullName,
            Email = newUser.Email,
            Role = newUser.Role,
            IsActive = newUser.IsActive
        };
    }

    public async Task<IEnumerable<UserResponse>> GetAllActiveUsersAsync()
    {
        var users = await _userRepository.GetAllActiveAsync();
        return users.Select(MapToResponse);
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        return user != null ? MapToResponse(user) : null;
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        user.FullName = request.FullName;
        user.MatriculationNumber = request.MatriculationNumber;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePasswordAsync(Guid id, UpdatePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("Invalid current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    private static UserRole DetermineUserRole(string email)
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

    private static UserResponse MapToResponse(User user)
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