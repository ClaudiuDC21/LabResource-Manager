namespace LabResource.Application.DTOs.Users;

public class RegisterUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? MatriculationNumber { get; set; }
    public string Password { get; set; } = string.Empty;
}