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

public class PickUpAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly PickUpAsset.Handler _handler;

    public PickUpAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new PickUpAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidApprovedBorrowing_ShouldSetActiveStatus()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Approved
        };

        var asset = new LabAsset
        {
            Id = assetId,
            Status = AssetStatus.Available
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new PickUpAsset.Command(borrowingId);

        await _handler.Handle(command, CancellationToken.None);

        record.Status.Should().Be(BorrowingStatus.Active);
        record.ActualBorrowedAt.Should().NotBeNull();
        asset.Status.Should().Be(AssetStatus.Borrowed);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidBorrowingId_ShouldThrowNotFoundException()
    {
        var borrowingId = Guid.NewGuid();
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var command = new PickUpAsset.Command(borrowingId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBorrowingIsNotApproved_ShouldThrowConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            Status = BorrowingStatus.Pending
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });

        var command = new PickUpAsset.Command(borrowingId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Reservation is not approved.");
    }

    [Fact]
    public async Task Handle_WhenAssetIsNotAvailable_ShouldThrowConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Approved
        };

        var asset = new LabAsset
        {
            Id = assetId,
            Status = AssetStatus.Defective
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new PickUpAsset.Command(borrowingId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Asset is not currently available.");
    }
}