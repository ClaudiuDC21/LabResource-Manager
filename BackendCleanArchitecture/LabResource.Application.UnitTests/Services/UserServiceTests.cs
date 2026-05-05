using FluentAssertions;
using LabResource.Application.DTOs.Users;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Domain.Exceptions;
using Moq;
using Xunit;

namespace LabResource.Application.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingEmail_ShouldThrowAlreadyExistsException()
    {
        var request = new RegisterUserRequest
        {
            Email = "test@yahoo.com",
            Password = "Password123!",
            FullName = "Test User"
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User());

        var action = async () => await _userService.RegisterUserAsync(request);

        await action.Should().ThrowAsync<AlreadyExistsException>();
    }

    [Fact]
    public async Task RegisterUserAsync_WithStandardEmail_ShouldAssignStudentRole()
    {
        var request = new RegisterUserRequest
        {
            Email = "student@gmail.com",
            Password = "Password123!",
            FullName = "John Doe",
            MatriculationNumber = "12345"
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var result = await _userService.RegisterUserAsync(request);

        result.Should().NotBeNull();
        result.Email.Should().Be("student@gmail.com");
        result.Role.Should().Be(UserRole.Student);

        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_WithUbbEmail_ShouldAssignTeacherRole()
    {
        var request = new RegisterUserRequest
        {
            Email = "profesor@ubbcluj.ro",
            Password = "Password123!",
            FullName = "Jane Doe"
        };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        var result = await _userService.RegisterUserAsync(request);

        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Teacher);

        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUserResponse()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FullName = "John Doe",
            Email = "john@example.com",
            Role = UserRole.Student,
            IsActive = true,
            MatriculationNumber = "98765"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        var result = await _userService.GetUserByIdAsync(userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.Role.Should().Be(UserRole.Student);
        result.IsActive.Should().BeTrue();
        result.MatriculationNumber.Should().Be("98765");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var act = async () => await _userService.GetUserByIdAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveUsers_AndMapCorrectly()
    {
        var activeUsers = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Active Student",
                Email = "student@test.com",
                Role = UserRole.Student,
                IsActive = true,
                MatriculationNumber = "12345"
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(activeUsers);

        var result = await _userService.GetAllActiveUsersAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var activeUser = result.First();
        activeUser.FullName.Should().Be("Active Student");
        activeUser.Email.Should().Be("student@test.com");
        activeUser.Role.Should().Be(UserRole.Student);
        activeUser.MatriculationNumber.Should().Be("12345");
        activeUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNoActiveUsersExist_ShouldReturnEmptyList()
    {
        _userRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(new List<User>());

        var result = await _userService.GetAllActiveUsersAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        _userRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(new List<User>());

        var result = await _userService.GetAllActiveUsersAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidId_ShouldUpdateUser()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FullName = "Old Name",
            MatriculationNumber = "OLD123"
        };
        var request = new UpdateUserRequest
        {
            FullName = "New Name",
            MatriculationNumber = "NEW999"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        var result = await _userService.UpdateUserAsync(userId, request);

        result.Should().BeTrue();
        existingUser.FullName.Should().Be("New Name");
        existingUser.MatriculationNumber.Should().Be("NEW999");
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = "New Name",
            MatriculationNumber = "NEW999"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var act = async () => await _userService.UpdateUserAsync(userId, request);

        await act.Should().ThrowAsync<NotFoundException>();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithValidPasswords_ShouldUpdateHash()
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

        var request = new UpdatePasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        await _userService.UpdatePasswordAsync(userId, request);

        BCrypt.Net.BCrypt.Verify(newPassword, existingUser.PasswordHash).Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithInvalidCurrentPassword_ShouldThrowBadRequestException()
    {
        var userId = Guid.NewGuid();
        var realPassword = "RealPassword123!";
        var currentHashedPassword = BCrypt.Net.BCrypt.HashPassword(realPassword);

        var existingUser = new User
        {
            Id = userId,
            PasswordHash = currentHashedPassword
        };

        var request = new UpdatePasswordRequest
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword456!"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        var action = async () => await _userService.UpdatePasswordAsync(userId, request);

        await action.Should().ThrowAsync<BadRequestException>().WithMessage("Invalid current password.");
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var request = new UpdatePasswordRequest
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword456!"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var action = async () => await _userService.UpdatePasswordAsync(userId, request);

        await action.Should().ThrowAsync<NotFoundException>();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldDeactivateUser()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            IsActive = true
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        var result = await _userService.DeactivateUserAsync(userId);

        result.Should().BeTrue();
        existingUser.IsActive.Should().BeFalse();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var act = async () => await _userService.DeactivateUserAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}