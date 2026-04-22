using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Common.Settings;
using LabResource.VerticalApi.Features.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Moq.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.Auth;

public class LoginTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly JwtSettings _jwtSettings;
    private readonly Login.Handler _handler;

    public LoginTests()
    {
        // 1. Pregătim DB-ul
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        // 2. Pregătim setările JWT (Mocks)
        _jwtSettings = new JwtSettings
        {
            Key = "SuperSecretKeyForTestingPurposes123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        var jwtOptionsMock = Options.Create(_jwtSettings);

        // 3. Inițializăm Handler-ul
        _handler = new Login.Handler(_dbContextMock.Object, jwtOptionsMock);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnResultWithToken()
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

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });

        var command = new Login.Command(email, password);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FullName.Should().Be("Test User");
        result.Token.Should().NotBeNullOrEmpty();

        // Verificăm conținutul Token-ului exact cum am învățat!
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);

        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnNull()
    {
        var email = "test@test.com";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("RealPassword123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });

        var command = new Login.Command(email, "WrongPassword!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldReturnNull()
    {
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var command = new Login.Command("nonexistent@test.com", "AnyPassword123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }
}