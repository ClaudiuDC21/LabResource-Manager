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

public class GetActiveBorrowingsForUserTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetActiveBorrowingsForUser.Handler _handler;

    public GetActiveBorrowingsForUserTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new GetActiveBorrowingsForUser.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserAndActiveBorrowings_ShouldReturnMappedResult()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Test User" };
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId, Name = "Multimeter", SerialNumber = "MM-001" };

        var activeRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            LabAssetId = assetId,
            LabAsset = asset,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(3),
            Status = BorrowingStatus.Active,
            ActualReturnedAt = null
        };

        var approvedRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            LabAssetId = Guid.NewGuid(),
            LabAsset = new LabAsset { Name = "Oscilloscope" },
            Status = BorrowingStatus.Approved,
            ActualReturnedAt = null
        };

        var returnedRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            Status = BorrowingStatus.Returned,
            ActualReturnedAt = DateTime.UtcNow
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { activeRecord, approvedRecord, returnedRecord });

        var query = new GetActiveBorrowingsForUser.Query(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.BorrowingRecordId == activeRecord.Id);
        result.Should().Contain(r => r.BorrowingRecordId == approvedRecord.Id);
        result.Should().NotContain(r => r.Status == BorrowingStatus.Returned);
    }

    [Fact]
    public async Task Handle_WithInvalidUser_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var query = new GetActiveBorrowingsForUser.Query(userId);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNoActiveBorrowings_ShouldReturnEmptyList()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        var returnedRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = BorrowingStatus.Returned,
            ActualReturnedAt = DateTime.UtcNow
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { returnedRecord });

        var query = new GetActiveBorrowingsForUser.Query(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}