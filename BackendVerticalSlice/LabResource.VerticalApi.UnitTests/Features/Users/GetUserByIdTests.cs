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

public class GetUserByIdTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetUserById.Handler _handler;

    public GetUserByIdTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new GetUserById.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnMappedResult()
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

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { existingUser });

        var query = new GetUserById.Query(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.Role.Should().Be(UserRole.Student);
        result.IsActive.Should().BeTrue();
        result.MatriculationNumber.Should().Be("98765");
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var query = new GetUserById.Query(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}