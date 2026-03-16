using LabResource.Application.DTOs.Users;

namespace LabResource.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponse> LoginOrRegisterAsync(LoginOrRegisterRequest request);

    Task<IEnumerable<UserResponse>> GetAllActiveUsersAsync();

    Task<UserResponse?> GetUserByIdAsync(Guid id);

    Task<bool> UpdateUserAsync(Guid id, UpdateUserRequest request);

    Task<bool> DeactivateUserAsync(Guid id);
}
