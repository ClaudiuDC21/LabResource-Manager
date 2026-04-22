using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Borrowings;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.Borrowings;

public class ReturnAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly ReturnAsset.Handler _handler;

    public ReturnAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new ReturnAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotDefective_ShouldUpdateStatusToAvailableAndReturnResult()
    {
        var assetId = Guid.NewGuid();
        var activeBorrowingId = Guid.NewGuid();

        var activeBorrowing = new BorrowingRecord
        {
            Id = activeBorrowingId,
            LabAssetId = assetId,
            ReturnedAt = null
        };

        var asset = new LabAsset
        {
            Id = assetId,
            Name = "Microscope",
            Status = AssetStatus.Borrowed
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { activeBorrowing });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReturnAsset.Command(assetId, "All good", false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.BorrowingRecordId.Should().Be(activeBorrowingId);
        result.AssetName.Should().Be("Microscope");
        result.NewStatus.Should().Be(AssetStatus.Available);
        result.ReturnedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        activeBorrowing.ReturnedAt.Should().NotBeNull();
        activeBorrowing.Remarks.Should().Be("All good");
        asset.Status.Should().Be(AssetStatus.Available);

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDefective_ShouldUpdateStatusToDefectiveAndReturnResult()
    {
        var assetId = Guid.NewGuid();
        var activeBorrowing = new BorrowingRecord { Id = Guid.NewGuid(), LabAssetId = assetId, ReturnedAt = null };
        var asset = new LabAsset { Id = assetId, Name = "Microscope", Status = AssetStatus.Borrowed };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { activeBorrowing });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReturnAsset.Command(assetId, "Broken lens", true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(AssetStatus.Defective);
        asset.Status.Should().Be(AssetStatus.Defective);
        activeBorrowing.Remarks.Should().Be("Broken lens");
    }

    [Fact]
    public async Task Handle_WithNoActiveBorrowing_ShouldThrowInvalidOperationException()
    {
        var assetId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var command = new ReturnAsset.Command(assetId, null, false);

        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("No active borrowing record found for this asset.");
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAssetNotFound_ShouldThrowArgumentException()
    {
        var assetId = Guid.NewGuid();
        var activeBorrowing = new BorrowingRecord { Id = Guid.NewGuid(), LabAssetId = assetId, ReturnedAt = null };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { activeBorrowing });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new ReturnAsset.Command(assetId, null, false);

        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Asset not found.");
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}