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

public class UpdateLabAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly UpdateLabAsset.Handler _handler;

    public UpdateLabAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new UpdateLabAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateAsset()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "OLD-123" };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new UpdateLabAsset.Command(assetId, "New Name", "NEW-456", "New Location", null, false);

        await _handler.Handle(command, CancellationToken.None);

        existingAsset.Name.Should().Be("New Name");
        existingAsset.SerialNumber.Should().Be("NEW-456");
        existingAsset.Location.Should().Be("New Location");
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidTeacherAssignment_ShouldUpdateTeacher()
    {
        var assetId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var teacher = new User { Id = teacherId, Role = UserRole.Teacher };
        var existingAsset = new LabAsset { Id = assetId, Name = "Asset", SerialNumber = "SN-1" };

        var usersDbSetMock = new Mock<DbSet<User>>();
        usersDbSetMock.Setup(x => x.FindAsync(It.Is<object[]>(args => (Guid)args[0] == teacherId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        _dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new UpdateLabAsset.Command(assetId, "Name", "SN-1", "Loc", teacherId, false);

        await _handler.Handle(command, CancellationToken.None);

        existingAsset.AssignedTeacherId.Should().Be(teacherId);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateSerialNumber_ShouldThrowAlreadyExistsException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Asset 1", SerialNumber = "SN-001" };
        var otherAsset = new LabAsset { Id = Guid.NewGuid(), Name = "Asset 2", SerialNumber = "DUPLICATE" };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset, otherAsset });

        var command = new UpdateLabAsset.Command(assetId, "Updated Asset 1", "DUPLICATE", "Loc", null, false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AlreadyExistsException>();
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowNotFoundException()
    {
        var assetId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new UpdateLabAsset.Command(assetId, "New Name", "NEW-123", "Loc", null, false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonTeacherRole_ShouldThrowBadRequestException()
    {
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var student = new User { Id = userId, Role = UserRole.Student };
        var existingAsset = new LabAsset { Id = assetId, Name = "Asset", SerialNumber = "SN-1" };

        var usersDbSetMock = new Mock<DbSet<User>>();
        usersDbSetMock.Setup(x => x.FindAsync(It.Is<object[]>(args => (Guid)args[0] == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _dbContextMock.Setup(db => db.Users).Returns(usersDbSetMock.Object);
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new UpdateLabAsset.Command(assetId, "Name", "SN-1", "Loc", userId, false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}