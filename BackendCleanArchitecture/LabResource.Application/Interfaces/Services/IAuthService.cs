using LabResource.Application.DTOs.Auth;

namespace LabResource.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}