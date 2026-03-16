namespace LabResource.Application.DTOs;

public class LoginOrRegisterRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
