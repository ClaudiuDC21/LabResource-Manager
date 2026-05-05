//using FluentAssertions;
//using LabResource.VerticalApi.Common.Entities;
//using LabResource.VerticalApi.Common.Persistence;
//using LabResource.VerticalApi.Features.Users;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Xunit;

//namespace LabResource.VerticalApi.UnitTests.Features.Users;

//public class UpdateUserTests
//{
//    private readonly Mock<ApplicationDbContext> _dbContextMock;
//    private readonly UpdateUser.Handler _handler;

//    public UpdateUserTests()
//    {
//        var options = new DbContextOptions<ApplicationDbContext>();
//        _dbContextMock = new Mock<ApplicationDbContext>(options);

//        _handler = new UpdateUser.Handler(_dbContextMock.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidId_ShouldUpdateUserAndReturnTrue()
//    {
//        var userId = Guid.NewGuid();
//        var existingUser = new User
//        {
//            Id = userId,
//            FullName = "Old Name",
//            MatriculationNumber = "OLD123"
//        };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { existingUser });

//        var command = new UpdateUser.Command(userId, "New Name", "NEW999");

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeTrue();
//        existingUser.FullName.Should().Be("New Name");
//        existingUser.MatriculationNumber.Should().Be("NEW999");

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithInvalidId_ShouldReturnFalseAndNotSave()
//    {
//        var userId = Guid.NewGuid();

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

//        var command = new UpdateUser.Command(userId, "New Name", "NEW999");

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeFalse();

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }
//}