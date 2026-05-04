//using FluentAssertions;
//using LabResource.Application.DTOs.Users;
//using LabResource.Application.Interfaces.Repositories;
//using LabResource.Application.Services;
//using LabResource.Domain.Entities;
//using LabResource.Domain.Enums;
//using Moq;
//using Xunit;

//namespace LabResource.Application.UnitTests.Services;

//public class UserServiceTests
//{
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly UserService _userService;

//    public UserServiceTests()
//    {
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _userService = new UserService(_userRepositoryMock.Object);
//    }

//    [Fact]
//    public async Task RegisterUserAsync_WithExistingEmail_ShouldThrowArgumentException()
//    {
//        var request = new RegisterUserRequest { Email = "test@yahoo.com", Password = "Password123!", FullName = "Test User" };

//        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
//            .ReturnsAsync(new User());

//        Func<Task> action = async () => await _userService.RegisterUserAsync(request);

//        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Email is already in use.");
//    }

//    [Fact]
//    public async Task RegisterUserAsync_WithStandardEmail_ShouldAssignStudentRole()
//    {
//        var request = new RegisterUserRequest { Email = "student@gmail.com", Password = "Password123!", FullName = "John Doe" };

//        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
//            .ReturnsAsync((User?)null);

//        var result = await _userService.RegisterUserAsync(request);

//        result.Should().NotBeNull();
//        result.Email.Should().Be("student@gmail.com");
//        result.Role.Should().Be(UserRole.Student);

//        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
//        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task RegisterUserAsync_WithUbbEmail_ShouldAssignTeacherRole()
//    {
//        var request = new RegisterUserRequest { Email = "profesor@ubbcluj.ro", Password = "Password123!", FullName = "Jane Doe" };

//        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(request.Email))
//            .ReturnsAsync((User?)null);

//        var result = await _userService.RegisterUserAsync(request);

//        result.Should().NotBeNull();
//        result.Role.Should().Be(UserRole.Teacher);
//    }

//    [Fact]
//    public async Task GetAllActiveUsersAsync_ShouldReturnMappedUsers()
//    {
//        var users = new List<User>
//        {
//            new User { Id = Guid.NewGuid(), Email = "user1@test.com", FullName = "User 1", Role = UserRole.Student },
//            new User { Id = Guid.NewGuid(), Email = "user2@test.com", FullName = "User 2", Role = UserRole.Teacher }
//        };

//        _userRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
//            .ReturnsAsync(users);

//        var result = await _userService.GetAllActiveUsersAsync();

//        result.Should().NotBeNull();
//        result.Should().HaveCount(2);
//        result.First().Email.Should().Be("user1@test.com");
//    }

//    [Fact]
//    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
//    {
//        var userId = Guid.NewGuid();
//        var user = new User { Id = userId, Email = "test@test.com", FullName = "Test" };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync(user);

//        var result = await _userService.GetUserByIdAsync(userId);

//        result.Should().NotBeNull();
//        result!.Id.Should().Be(userId);
//    }

//    [Fact]
//    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
//    {
//        var userId = Guid.NewGuid();

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync((User?)null);

//        var result = await _userService.GetUserByIdAsync(userId);

//        result.Should().BeNull();
//    }

//    [Fact]
//    public async Task UpdateUserAsync_WithValidId_ShouldUpdateAndReturnTrue()
//    {
//        var userId = Guid.NewGuid();
//        var existingUser = new User { Id = userId, FullName = "Old Name" };
//        var request = new UpdateUserRequest { FullName = "New Name", MatriculationNumber = "12345" };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync(existingUser);

//        var result = await _userService.UpdateUserAsync(userId, request);

//        result.Should().BeTrue();
//        existingUser.FullName.Should().Be("New Name");
//        existingUser.MatriculationNumber.Should().Be("12345");

//        _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
//        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task UpdateUserAsync_WithInvalidId_ShouldReturnFalse()
//    {
//        var userId = Guid.NewGuid();
//        var request = new UpdateUserRequest { FullName = "New Name" };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync((User?)null);

//        var result = await _userService.UpdateUserAsync(userId, request);

//        result.Should().BeFalse();
//        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task UpdatePasswordAsync_WithValidPassword_ShouldUpdateAndReturnTrue()
//    {
//        var userId = Guid.NewGuid();
//        var currentPassword = "OldPassword123!";
//        var newPassword = "NewPassword456!";
//        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);

//        var existingUser = new User { Id = userId, PasswordHash = hashedPassword };
//        var request = new UpdatePasswordRequest { CurrentPassword = currentPassword, NewPassword = newPassword };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync(existingUser);

//        var result = await _userService.UpdatePasswordAsync(userId, request);

//        result.Should().BeTrue();
//        BCrypt.Net.BCrypt.Verify(newPassword, existingUser.PasswordHash).Should().BeTrue();

//        _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
//        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task UpdatePasswordAsync_WithInvalidCurrentPassword_ShouldThrowArgumentException()
//    {
//        var userId = Guid.NewGuid();
//        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("RealPassword123!");

//        var existingUser = new User { Id = userId, PasswordHash = hashedPassword };
//        var request = new UpdatePasswordRequest { CurrentPassword = "WrongPassword!", NewPassword = "NewPassword456!" };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync(existingUser);

//        Func<Task> action = async () => await _userService.UpdatePasswordAsync(userId, request);

//        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid current password.");
//        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
//    }

//    [Fact]
//    public async Task UpdatePasswordAsync_WithInvalidId_ShouldReturnFalse()
//    {
//        var userId = Guid.NewGuid();
//        var request = new UpdatePasswordRequest { CurrentPassword = "Old", NewPassword = "New" };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync((User?)null);

//        var result = await _userService.UpdatePasswordAsync(userId, request);

//        result.Should().BeFalse();
//    }

//    [Fact]
//    public async Task DeactivateUserAsync_WithValidId_ShouldSetIsActiveToFalseAndReturnTrue()
//    {
//        var userId = Guid.NewGuid();
//        var existingUser = new User { Id = userId, IsActive = true };

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync(existingUser);

//        var result = await _userService.DeactivateUserAsync(userId);

//        result.Should().BeTrue();
//        existingUser.IsActive.Should().BeFalse();

//        _userRepositoryMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
//        _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
//    }

//    [Fact]
//    public async Task DeactivateUserAsync_WithInvalidId_ShouldReturnFalse()
//    {
//        var userId = Guid.NewGuid();

//        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
//            .ReturnsAsync((User?)null);

//        var result = await _userService.DeactivateUserAsync(userId);

//        result.Should().BeFalse();
//        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
//    }
//}