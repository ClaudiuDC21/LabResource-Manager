using LabResource.Application.DTOs.Users;
using LabResource.Domain.Entities;

namespace LabResource.Application.Mappings;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            MatriculationNumber = user.MatriculationNumber,
            IsActive = user.IsActive
        };
    }
}