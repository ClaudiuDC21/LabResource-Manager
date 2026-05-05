using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
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
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _jwtSettings = new JwtSettings
        {
            Key = "SuperSecretKeyForTestingPurposes123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        var jwtOptionsMock = Options.Create(_jwtSettings);

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
            Role = UserRole.Student,
            IsActive = true
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });

        var command = new Login.Command(email, password);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.FullName.Should().Be("Test User");
        result.Token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Token);

        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowBadRequestException()
    {
        var email = "test@test.com";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("RealPassword123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hashedPassword,
            IsActive = true
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });

        var command = new Login.Command(email, "WrongPassword!");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldThrowBadRequestException()
    {
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var command = new Login.Command("nonexistent@test.com", "AnyPassword123!");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowForbiddenAccessException()
    {
        var email = "inactive@test.com";
        var password = "ValidPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = "Inactive User",
            PasswordHash = hashedPassword,
            IsActive = false
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });

        var command = new Login.Command(email, password);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Your account has been deactivated.");
    }
}