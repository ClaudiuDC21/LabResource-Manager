using LabResource.Application.DTOs.Auth;
using LabResource.Domain.Entities;

namespace LabResource.Application.Mappings;

public static class AuthMapper
{
    public static AuthResponse ToAuthResponse(this User user, string token)
    {
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
}