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

public class GetPendingRequestsForTeacherTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetPendingRequestsForTeacher.Handler _handler;

    public GetPendingRequestsForTeacherTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new GetPendingRequestsForTeacher.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithPendingRequestsForTeacher_ShouldReturnMappedResults()
    {
        var teacherId = Guid.NewGuid();
        var student = new User { Id = Guid.NewGuid(), FullName = "Student Name" };

        var assetForTeacher = new LabAsset
        {
            Id = Guid.NewGuid(),
            Name = "Oscilloscope",
            AssignedTeacherId = teacherId
        };

        var pendingRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetForTeacher.Id,
            LabAsset = assetForTeacher,
            UserId = student.Id,
            User = student,
            Status = BorrowingStatus.Pending,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(2)
        };

        var approvedRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetForTeacher.Id,
            LabAsset = assetForTeacher,
            Status = BorrowingStatus.Approved
        };

        var otherAsset = new LabAsset
        {
            Id = Guid.NewGuid(),
            AssignedTeacherId = Guid.NewGuid()
        };

        var otherPendingRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = otherAsset.Id,
            LabAsset = otherAsset,
            Status = BorrowingStatus.Pending
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>
        {
            pendingRecord,
            approvedRecord,
            otherPendingRecord
        });

        var query = new GetPendingRequestsForTeacher.Query(teacherId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().BorrowingRecordId.Should().Be(pendingRecord.Id);
        result.First().AssetName.Should().Be("Oscilloscope");
        result.First().UserName.Should().Be("Student Name");
    }

    [Fact]
    public async Task Handle_WhenNoPendingRequestsExist_ShouldReturnEmptyList()
    {
        var teacherId = Guid.NewGuid();
        var asset = new LabAsset { Id = Guid.NewGuid(), AssignedTeacherId = teacherId };

        var approvedRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = asset.Id,
            LabAsset = asset,
            Status = BorrowingStatus.Approved
        };

        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { approvedRecord });

        var query = new GetPendingRequestsForTeacher.Query(teacherId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var query = new GetPendingRequestsForTeacher.Query(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}