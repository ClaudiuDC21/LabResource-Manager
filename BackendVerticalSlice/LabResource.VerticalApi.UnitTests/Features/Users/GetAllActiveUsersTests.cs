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

public class GetAllActiveUsersTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetAllActiveUsers.Handler _handler;

    public GetAllActiveUsersTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new GetAllActiveUsers.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveUsers_AndMapCorrectly()
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Active Student",
                Email = "student@test.com",
                Role = UserRole.Student,
                IsActive = true,
                MatriculationNumber = "12345"
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Inactive Teacher",
                Email = "teacher@test.com",
                Role = UserRole.Teacher,
                IsActive = false
            }
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(users);

        var query = new GetAllActiveUsers.Query();

        var result = await _handler.Handle(query, CancellationToken.None);

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
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "Inactive 1", IsActive = false },
            new User { Id = Guid.NewGuid(), FullName = "Inactive 2", IsActive = false }
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(users);

        var query = new GetAllActiveUsers.Query();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var query = new GetAllActiveUsers.Query();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}