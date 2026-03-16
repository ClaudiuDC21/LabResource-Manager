namespace LabResource.Application.DTOs.Users;

public class LoginOrRegisterRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
