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

public class ReviewRequestTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly ReviewRequest.Handler _handler;

    public ReviewRequestTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new ReviewRequest.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidApproval_ShouldSetApprovedStatus()
    {
        var borrowingId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Pending
        };

        var asset = new LabAsset
        {
            Id = assetId,
            AssignedTeacherId = teacherId
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReviewRequest.Command(borrowingId, teacherId, true, "Approved notes");

        await _handler.Handle(command, CancellationToken.None);

        record.Status.Should().Be(BorrowingStatus.Approved);
        record.Remarks.Should().Be("Approved notes");
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRejection_ShouldSetRejectedStatusAndMakeAssetAvailable()
    {
        var borrowingId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Pending
        };

        var asset = new LabAsset
        {
            Id = assetId,
            AssignedTeacherId = teacherId,
            Status = AssetStatus.PendingApproval
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReviewRequest.Command(borrowingId, teacherId, false, "Rejected notes");

        await _handler.Handle(command, CancellationToken.None);

        record.Status.Should().Be(BorrowingStatus.Rejected);
        record.Remarks.Should().Be("Rejected notes");
        asset.Status.Should().Be(AssetStatus.Available);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidBorrowingId_ShouldThrowNotFoundException()
    {
        var borrowingId = Guid.NewGuid();
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var command = new ReviewRequest.Command(borrowingId, Guid.NewGuid(), true, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotPending_ShouldThrowConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            Status = BorrowingStatus.Approved
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });

        var command = new ReviewRequest.Command(borrowingId, Guid.NewGuid(), true, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Only pending requests can be reviewed.");
    }

    [Fact]
    public async Task Handle_WhenTeacherIsNotAssigned_ShouldThrowForbiddenAccessException()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var realTeacherId = Guid.NewGuid();
        var wrongTeacherId = Guid.NewGuid();

        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Pending
        };

        var asset = new LabAsset
        {
            Id = assetId,
            AssignedTeacherId = realTeacherId
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record });
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

        var command = new ReviewRequest.Command(borrowingId, wrongTeacherId, true, null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}