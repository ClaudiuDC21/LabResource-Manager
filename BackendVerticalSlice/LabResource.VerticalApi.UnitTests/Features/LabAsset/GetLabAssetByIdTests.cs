//using FluentAssertions;
//using LabResource.VerticalApi.Common.Entities;
//using LabResource.VerticalApi.Common.Enums;
//using LabResource.VerticalApi.Common.Persistence;
//using LabResource.VerticalApi.Features.LabAssets;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Xunit;

//namespace LabResource.VerticalApi.UnitTests.Features.LabAssets;

//public class GetLabAssetByIdTests
//{
//    private readonly Mock<ApplicationDbContext> _dbContextMock;
//    private readonly GetLabAssetById.Handler _handler;

//    public GetLabAssetByIdTests()
//    {
//        var options = new DbContextOptions<ApplicationDbContext>();
//        _dbContextMock = new Mock<ApplicationDbContext>(options);

//        _handler = new GetLabAssetById.Handler(_dbContextMock.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidIdAndActiveBorrowing_ShouldReturnMappedResultWithBorrower()
//    {
//        var assetId = Guid.NewGuid();
//        var user = new User { Id = Guid.NewGuid(), FullName = "Jane Doe" };

//        var activeBorrowing = new BorrowingRecord
//        {
//            Id = Guid.NewGuid(),
//            ReturnedAt = null,
//            User = user
//        };

//        var asset = new LabAsset
//        {
//            Id = assetId,
//            Name = "Oscilloscope",
//            SerialNumber = "OSC-123",
//            Status = AssetStatus.Borrowed,
//            IsActive = true,
//            BorrowingRecords = new List<BorrowingRecord> { activeBorrowing }
//        };

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

//        var query = new GetLabAssetById.Query(assetId);

//        var result = await _handler.Handle(query, CancellationToken.None);

//        result.Should().NotBeNull();
//        result!.Id.Should().Be(assetId);
//        result.Name.Should().Be("Oscilloscope");
//        result.Status.Should().Be(AssetStatus.Borrowed);
//        result.CurrentBorrowerName.Should().Be("Jane Doe");
//    }

//    [Fact]
//    public async Task Handle_WithValidIdAndNoActiveBorrowing_ShouldReturnResultWithNullBorrower()
//    {
//        var assetId = Guid.NewGuid();

//        var pastBorrowing = new BorrowingRecord
//        {
//            Id = Guid.NewGuid(),
//            ReturnedAt = DateTime.UtcNow,
//            User = new User { FullName = "Old Borrower" }
//        };

//        var asset = new LabAsset
//        {
//            Id = assetId,
//            Name = "Multimeter",
//            Status = AssetStatus.Available,
//            IsActive = true,
//            BorrowingRecords = new List<BorrowingRecord> { pastBorrowing }
//        };

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

//        var query = new GetLabAssetById.Query(assetId);

//        var result = await _handler.Handle(query, CancellationToken.None);

//        result.Should().NotBeNull();
//        result!.Id.Should().Be(assetId);
//        result.CurrentBorrowerName.Should().BeNull();
//    }

//    [Fact]
//    public async Task Handle_WithInvalidId_ShouldReturnNull()
//    {
//        var assetId = Guid.NewGuid();

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

//        var query = new GetLabAssetById.Query(assetId);

//        var result = await _handler.Handle(query, CancellationToken.None);

//        result.Should().BeNull();
//    }
//}