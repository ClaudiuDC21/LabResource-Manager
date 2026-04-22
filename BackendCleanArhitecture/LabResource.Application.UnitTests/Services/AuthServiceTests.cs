using FluentAssertions;
using LabResource.Application.DTOs.Auth;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Application.Settings;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace LabResource.Application.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();

        _jwtSettings = new JwtSettings
        {
            Key = "SuperSecretKeyForTestingPurposes123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        var jwtOptionsMock = Options.Create(_jwtSettings);

        _authService = new AuthService(_userRepositoryMock.Object, jwtOptionsMock);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponseWithToken()
    {
        var email = "test@test.com";
        var password = "ValidPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = "Test User",
            PasswordHash = hashedPassword,
            Role = UserRole.Student
        };

        var request = new LoginRequest { Email = email, Password = password };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FullName.Should().Be("Test User");
        result.Token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);

        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
    {
        var email = "test@test.com";
        var password = "ValidPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword
        };

        var request = new LoginRequest { Email = email, Password = "WrongPassword!" };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        var request = new LoginRequest { Email = "nonexistent@test.com", Password = "AnyPassword123!" };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(request);

        result.Should().BeNull();
    }
}