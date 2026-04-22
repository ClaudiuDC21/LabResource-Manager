using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Borrowings;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.Borrowings;

public class GetAssetHistoryTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetAssetHistory.Handler _handler;

    public GetAssetHistoryTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new GetAssetHistory.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidAssetAndHistory_ShouldReturnMappedAndOrderedResult()
    {
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId };

        var user = new User { Id = Guid.NewGuid(), FullName = "Alice Smith", MatriculationNumber = "ALICE123" };

        var olderRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetId,
            UserId = user.Id,
            User = user,
            BorrowedAt = DateTime.UtcNow.AddDays(-10),
            ReturnedAt = DateTime.UtcNow.AddDays(-8),
            Remarks = "Returned safely"
        };

        var newerRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetId,
            UserId = user.Id,
            User = user,
            BorrowedAt = DateTime.UtcNow.AddDays(-2),
            ReturnedAt = null,
            Remarks = null
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { olderRecord, newerRecord });

        var query = new GetAssetHistory.Query(assetId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First();
        firstResult.BorrowingRecordId.Should().Be(newerRecord.Id);
        firstResult.BorrowedAt.Should().Be(newerRecord.BorrowedAt);
        firstResult.UserName.Should().Be("Alice Smith");

        var secondResult = result.Last();
        secondResult.BorrowingRecordId.Should().Be(olderRecord.Id);
        secondResult.Remarks.Should().Be("Returned safely");
    }

    [Fact]
    public async Task Handle_WithInvalidAsset_ShouldThrowArgumentException()
    {
        var assetId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var query = new GetAssetHistory.Query(assetId);

        Func<Task> action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Asset not found.");
    }

    [Fact]
    public async Task Handle_WithNoHistory_ShouldReturnEmptyList()
    {
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });
        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

        var query = new GetAssetHistory.Query(assetId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}