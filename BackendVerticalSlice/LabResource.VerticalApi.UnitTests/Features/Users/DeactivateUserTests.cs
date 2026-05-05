//using FluentAssertions;
//using LabResource.VerticalApi.Common.Entities;
//using LabResource.VerticalApi.Common.Persistence;
//using LabResource.VerticalApi.Features.Users;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Xunit;

//namespace LabResource.VerticalApi.UnitTests.Features.Users;

//public class DeactivateUserTests
//{
//    private readonly Mock<ApplicationDbContext> _dbContextMock;
//    private readonly DeactivateUser.Handler _handler;

//    public DeactivateUserTests()
//    {
//        var options = new DbContextOptions<ApplicationDbContext>();
//        _dbContextMock = new Mock<ApplicationDbContext>(options);

//        _handler = new DeactivateUser.Handler(_dbContextMock.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidId_ShouldDeactivateUserAndReturnTrue()
//    {
//        var userId = Guid.NewGuid();
//        var existingUser = new User
//        {
//            Id = userId,
//            IsActive = true
//        };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { existingUser });

//        var command = new DeactivateUser.Command(userId);

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeTrue();
//        existingUser.IsActive.Should().BeFalse();

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithInvalidId_ShouldReturnFalseAndNotSave()
//    {
//        var userId = Guid.NewGuid();

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

//        var command = new DeactivateUser.Command(userId);

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeFalse();

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }
//}