using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.Users;

public class RegisterUserTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly RegisterUser.Handler _handler;

    public RegisterUserTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new RegisterUser.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowArgumentException()
    {
        var existingUsers = new List<User>
        {
            new User { Email = "test@yahoo.com" }
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(existingUsers);

        var command = new RegisterUser.Command("Test Name", "test@yahoo.com", null, "Password123!");

        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Email is already in use.");
    }

    [Fact]
    public async Task Handle_WithStandardEmail_ShouldAssignStudentRole()
    {
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var command = new RegisterUser.Command("John Doe", "student@gmail.com", "12345", "Password123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Student);
        result.Email.Should().Be("student@gmail.com");

        _dbContextMock.Verify(db => db.Users.Add(It.IsAny<User>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUbbEmail_ShouldAssignTeacherRole()
    {
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var command = new RegisterUser.Command("Jane Doe", "profesor@ubbcluj.ro", null, "Password123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Teacher);
    }
}