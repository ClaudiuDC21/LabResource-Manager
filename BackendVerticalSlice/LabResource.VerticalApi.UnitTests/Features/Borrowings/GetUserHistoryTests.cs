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

public class GetUserHistoryTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetUserHistory.Handler _handler;

    public GetUserHistoryTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);
        _handler = new GetUserHistory.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserAndHistory_ShouldReturnMappedAndOrderedResults()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var asset = new LabAsset { Id = Guid.NewGuid(), Name = "Asset 1", SerialNumber = "SN1", Status = AssetStatus.Available };

        var record1 = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LabAssetId = asset.Id,
            LabAsset = asset,
            RequestedStartDate = DateTime.UtcNow.AddDays(-10),
            RequestedEndDate = DateTime.UtcNow.AddDays(-5),
            ActualReturnedAt = DateTime.UtcNow.AddDays(-5),
            Status = BorrowingStatus.Returned,
            Remarks = "All good"
        };

        var record2 = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LabAssetId = asset.Id,
            LabAsset = asset,
            RequestedStartDate = DateTime.UtcNow.AddDays(-2),
            RequestedEndDate = DateTime.UtcNow.AddDays(2),
            ActualReturnedAt = null,
            Status = BorrowingStatus.Active,
            Remarks = null
        };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { record1, record2 });

        var query = new GetUserHistory.Query(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First();
        firstResult.RequestedStartDate.Should().Be(record2.RequestedStartDate);
        firstResult.AssetName.Should().Be("Asset 1");
        firstResult.Status.Should().Be(BorrowingStatus.Active);

        var secondResult = result.Last();
        secondResult.RequestedStartDate.Should().Be(record1.RequestedStartDate);
        secondResult.Remarks.Should().Be("All good");
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

        var query = new GetUserHistory.Query(userId);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithValidUserAndNoHistory_ShouldReturnEmptyList()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var query = new GetUserHistory.Query(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}