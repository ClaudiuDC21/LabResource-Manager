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
    public async Task Handle_WithValidActiveBorrowing_ShouldReturnResultAndUpdateStatus()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Active,
            Remarks = "Initial remark"
        };

        var asset = new LabAsset
        {
            Id = assetId,
            Name = "Microscope",
            Status = AssetStatus.Borrowed
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReturnAsset.Command(borrowingId, "Returned in good condition", false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.NewStatus.Should().Be(AssetStatus.Available);
        record.Status.Should().Be(BorrowingStatus.Returned);
        record.ActualReturnedAt.Should().NotBeNull();
        record.Remarks.Should().Contain("Return Note: Returned in good condition");
        asset.Status.Should().Be(AssetStatus.Available);

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDefective_ShouldSetAssetStatusToDefective()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var record = new BorrowingRecord { Id = borrowingId, LabAssetId = assetId, Status = BorrowingStatus.Active };
        var asset = new LabAsset { Id = assetId, Name = "Microscope", Status = AssetStatus.Borrowed };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReturnAsset.Command(borrowingId, "Broken lens", true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.NewStatus.Should().Be(AssetStatus.Defective);
        asset.Status.Should().Be(AssetStatus.Defective);
    }

    [Fact]
    public async Task Handle_WithInvalidBorrowingId_ShouldThrowNotFoundException()
    {
        var borrowingId = Guid.NewGuid();
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var command = new ReturnAsset.Command(borrowingId, null, false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenBorrowingIsNotActive_ShouldThrowConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var record = new BorrowingRecord { Id = borrowingId, Status = BorrowingStatus.Returned };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });

        var command = new ReturnAsset.Command(borrowingId, null, false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Borrowing is not active.");
    }
}