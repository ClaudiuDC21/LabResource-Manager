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

    private const string InvalidUserIdMessage = "The provided user ID is invalid.";
    private const string UserEntityName = "User";

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> RegisterUserAsync(RegisterUserRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new AlreadyExistsException(UserEntityName, request.Email);
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
            throw new BadRequestException(InvalidUserIdMessage);
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException(UserEntityName, id);
        }

        return user.ToResponse();
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException(InvalidUserIdMessage);
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException(UserEntityName, id);
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
            throw new BadRequestException(InvalidUserIdMessage);
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException(UserEntityName, id);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Invalid current password.");
        }

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
            throw new BadRequestException(InvalidUserIdMessage);
        }

        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException(UserEntityName, id);
        }

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