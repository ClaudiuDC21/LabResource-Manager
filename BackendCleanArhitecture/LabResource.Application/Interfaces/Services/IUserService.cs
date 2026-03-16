using LabResource.Application.DTOs;
using LabResource.Application.DTOs.Users;

namespace LabResource.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponse> LoginOrRegisterAsync(LoginOrRegisterRequest request);
}
