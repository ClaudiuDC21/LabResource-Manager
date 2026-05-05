using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Borrowings;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.Borrowings;

public class RequestAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly RequestAsset.Handler _handler;

    public RequestAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new RequestAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreatePendingRequest()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "John Doe", IsActive = true };
        var asset = new LabAsset { Id = assetId, Name = "Oscilloscope", Status = AssetStatus.Available, IsActive = true, AssignedTeacherId = Guid.NewGuid() };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var command = new RequestAsset.Command(userId, assetId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BorrowingStatus.Pending);
        asset.Status.Should().Be(AssetStatus.PendingApproval);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsAssignedTeacher_ShouldCreateApprovedRequest()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Prof Smith", IsActive = true };
        var asset = new LabAsset { Id = assetId, Name = "Oscilloscope", Status = AssetStatus.Available, IsActive = true, AssignedTeacherId = userId };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var command = new RequestAsset.Command(userId, assetId, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(BorrowingStatus.Approved);
        asset.Status.Should().Be(AssetStatus.Borrowed);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = false };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });

        var command = new RequestAsset.Command(userId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDefectiveAsset_ShouldThrowConflictException()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        var asset = new LabAsset { Id = assetId, Status = AssetStatus.Defective, IsActive = true };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new RequestAsset.Command(userId, assetId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Asset is defective.");
    }

    [Fact]
    public async Task Handle_WithOverlappingBooking_ShouldThrowConflictException()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        var asset = new LabAsset { Id = assetId, Status = AssetStatus.Available, IsActive = true };

        var existingRecord = new BorrowingRecord
        {
            LabAssetId = assetId,
            Status = BorrowingStatus.Approved,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(5)
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { existingRecord });

        var command = new RequestAsset.Command(userId, assetId, DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3));

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Asset is already booked for this period.");
    }
}