//using FluentAssertions;
//using LabResource.VerticalApi.Common.Entities;
//using LabResource.VerticalApi.Common.Enums;
//using LabResource.VerticalApi.Common.Persistence;
//using LabResource.VerticalApi.Features.Borrowings;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Xunit;

//namespace LabResource.VerticalApi.UnitTests.Features.Borrowings;

//public class BorrowAssetTests
//{
//    private readonly Mock<ApplicationDbContext> _dbContextMock;
//    private readonly RequestAsset.Handler _handler;

//    public BorrowAssetTests()
//    {
//        var options = new DbContextOptions<ApplicationDbContext>();
//        _dbContextMock = new Mock<ApplicationDbContext>(options);

//        _handler = new RequestAsset.Handler(_dbContextMock.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidData_ShouldCreateBorrowingAndReturnResult()
//    {
//        var userId = Guid.NewGuid();
//        var assetId = Guid.NewGuid();

//        var user = new User { Id = userId, FullName = "John Doe", IsActive = true };
//        var asset = new LabAsset { Id = assetId, Name = "Oscilloscope", Status = AssetStatus.Available, IsActive = true };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });
//        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord>());

//        var command = new RequestAsset.Command(userId, assetId);

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().NotBeNull();
//        result.UserId.Should().Be(userId);
//        result.LabAssetId.Should().Be(assetId);
//        result.UserName.Should().Be("John Doe");
//        result.AssetName.Should().Be("Oscilloscope");

//        asset.Status.Should().Be(AssetStatus.Borrowed);

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithInactiveUser_ShouldThrowArgumentException()
//    {
//        var userId = Guid.NewGuid();
//        var assetId = Guid.NewGuid();

//        var user = new User { Id = userId, IsActive = false };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

//        var command = new RequestAsset.Command(userId, assetId);

//        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

//        await action.Should().ThrowAsync<ArgumentException>().WithMessage("User not found or inactive.");
//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }

//    [Fact]
//    public async Task Handle_WithUnavailableAsset_ShouldThrowInvalidOperationException()
//    {
//        var userId = Guid.NewGuid();
//        var assetId = Guid.NewGuid();

//        var user = new User { Id = userId, IsActive = true };
//        var asset = new LabAsset { Id = assetId, Status = AssetStatus.Defective, IsActive = true };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

//        var command = new RequestAsset.Command(userId, assetId);

//        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

//        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage($"Asset is currently not available. Current status: {AssetStatus.Defective}");
//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }

//    [Fact]
//    public async Task Handle_WithInactiveAsset_ShouldThrowArgumentException()
//    {
//        var userId = Guid.NewGuid();
//        var assetId = Guid.NewGuid();

//        var user = new User { Id = userId, IsActive = true };
//        var asset = new LabAsset { Id = assetId, IsActive = false };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { asset });

//        var command = new RequestAsset.Command(userId, assetId);

//        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

//        await action.Should().ThrowAsync<ArgumentException>().WithMessage("Asset not found or inactive.");
//    }
//}