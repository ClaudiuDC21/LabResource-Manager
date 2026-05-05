using LabResource.Application.DTOs.Users;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Interfaces.Services;
using LabResource.Application.Mappings;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Domain.Exceptions;

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
            // Hash the password using BCrypt before storing it in the database
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return newUser.ToResponse();
    }

    public async Task<IEnumerable<UserResponse>> GetAllActiveUsersAsync()
    {
        var users = await _userRepository.GetAllActiveAsync();
        return users.Select(user => user.ToResponse());
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided user ID is invalid.");
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        return user.ToResponse();
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided user ID is invalid.");
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        user.FullName = request.FullName;
        user.MatriculationNumber = request.MatriculationNumber;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePasswordAsync(Guid id, UpdatePasswordRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided user ID is invalid.");
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Invalid current password.");
        }

        // Prevent updating if the new password is the same as the old one
        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            throw new ConflictException("The new password cannot be the same as the current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException("The provided user ID is invalid.");
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        // Prevent deactivating an already deactivated user
        if (!user.IsActive)
        {
            throw new ConflictException("This user is already deactivated.");
        }

        user.IsActive = false;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}