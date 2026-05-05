using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.LabAssets;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.LabAssets;

public class CreateLabAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly CreateLabAsset.Handler _handler;

    public CreateLabAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new CreateLabAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidDataAndNoTeacher_ShouldCreateAndReturnResult()
    {
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new CreateLabAsset.Command("Oscilloscope", "SN-12345", "Room A", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Oscilloscope");
        result.SerialNumber.Should().Be("SN-12345");
        result.Status.Should().Be(AssetStatus.Available);
        result.IsActive.Should().BeTrue();

        _dbContextMock.Verify(db => db.LabAssets.AddAsync(It.IsAny<LabAsset>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidTeacher_ShouldCreateAndReturnResult()
    {
        var teacherId = Guid.NewGuid();
        var teacher = new User { Id = teacherId, Role = UserRole.Teacher };

        var usersDbSetMock = new Mock<DbSet<User>>();
        usersDbSetMock.Setup(x => x.FindAsync(It.Is<object[]>(args => (Guid)args[0] == teacherId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        _dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new CreateLabAsset.Command("Oscilloscope", "SN-12345", "Room A", teacherId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AssignedTeacherId.Should().Be(teacherId);
    }

    [Fact]
    public async Task Handle_WithInvalidTeacherId_ShouldThrowNotFoundException()
    {
        var teacherId = Guid.NewGuid();

        var usersDbSetMock = new Mock<DbSet<User>>();
        usersDbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);

        var command = new CreateLabAsset.Command("Oscilloscope", "SN-12345", "Room A", teacherId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonTeacherRole_ShouldThrowBadRequestException()
    {
        var teacherId = Guid.NewGuid();
        var student = new User { Id = teacherId, Role = UserRole.Student };

        var usersDbSetMock = new Mock<DbSet<User>>();
        usersDbSetMock.Setup(x => x.FindAsync(It.Is<object[]>(args => (Guid)args[0] == teacherId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);

        var command = new CreateLabAsset.Command("Oscilloscope", "SN-12345", "Room A", teacherId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithExistingSerialNumber_ShouldThrowAlreadyExistsException()
    {
        var existingAsset = new LabAsset { Id = Guid.NewGuid(), Name = "Old Osc", SerialNumber = "DUPLICATE-SN" };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new CreateLabAsset.Command("New Osc", "DUPLICATE-SN", null, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_WithNullSerialNumber_ShouldCreateAndReturnResult()
    {
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new CreateLabAsset.Command("Pack of Resistors", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SerialNumber.Should().BeNull();

        _dbContextMock.Verify(db => db.LabAssets.AddAsync(It.IsAny<LabAsset>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}