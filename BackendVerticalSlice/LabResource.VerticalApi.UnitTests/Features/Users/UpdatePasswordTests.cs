using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.Users;

public class UpdatePasswordTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly UpdatePassword.Handler _handler;

    public UpdatePasswordTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new UpdatePassword.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPasswords_ShouldUpdateHashAndReturnTrue()
    {
        var userId = Guid.NewGuid();
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword456!";
        var currentHashedPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);

        var existingUser = new User
        {
            Id = userId,
            PasswordHash = currentHashedPassword
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { existingUser });

        var command = new UpdatePassword.Command(userId, currentPassword, newPassword);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify(newPassword, existingUser.PasswordHash).Should().BeTrue();

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCurrentPassword_ShouldThrowArgumentException()
    {
        var userId = Guid.NewGuid();
        var realPassword = "RealPassword123!";
        var currentHashedPassword = BCrypt.Net.BCrypt.HashPassword(realPassword);

        var existingUser = new User
        {
            Id = userId,
            PasswordHash = currentHashedPassword
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { existingUser });

        var command = new UpdatePassword.Command(userId, "WrongPassword!", "NewPassword456!");

        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid current password.");

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var command = new UpdatePassword.Command(userId, "OldPassword123!", "NewPassword456!");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}