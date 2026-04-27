//using FluentAssertions;
//using LabResource.VerticalApi.Common.Entities;
//using LabResource.VerticalApi.Common.Persistence;
//using LabResource.VerticalApi.Features.Borrowings;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Xunit;

//namespace LabResource.VerticalApi.UnitTests.Features.Borrowings;

//public class GetActiveBorrowingsForUserTests
//{
//    private readonly Mock<ApplicationDbContext> _dbContextMock;
//    private readonly GetActiveBorrowingsForUser.Handler _handler;

//    public GetActiveBorrowingsForUserTests()
//    {
//        var options = new DbContextOptions<ApplicationDbContext>();
//        _dbContextMock = new Mock<ApplicationDbContext>(options);

//        _handler = new GetActiveBorrowingsForUser.Handler(_dbContextMock.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidUserAndActiveBorrowings_ShouldReturnMappedResult()
//    {
//        var userId = Guid.NewGuid();
//        var user = new User { Id = userId };

//        var assetId = Guid.NewGuid();
//        var asset = new LabAsset { Id = assetId, Name = "Multimeter", SerialNumber = "MM-001" };

//        var activeRecord = new BorrowingRecord
//        {
//            Id = Guid.NewGuid(),
//            UserId = userId,
//            LabAssetId = assetId,
//            LabAsset = asset,
//            BorrowedAt = DateTime.UtcNow.AddDays(-2),
//            ReturnedAt = null
//        };

//        var returnedRecord = new BorrowingRecord
//        {
//            Id = Guid.NewGuid(),
//            UserId = userId,
//            LabAssetId = Guid.NewGuid(),
//            LabAsset = new LabAsset { Name = "Old Cables" },
//            BorrowedAt = DateTime.UtcNow.AddDays(-10),
//            ReturnedAt = DateTime.UtcNow.AddDays(-5)
//        };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
//        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { activeRecord, returnedRecord });

//        var query = new GetActiveBorrowingsForUser.Query(userId);

//        var result = await _handler.Handle(query, CancellationToken.None);

//        result.Should().NotBeNull();
//        result.Should().HaveCount(1);

//        var activeBorrowing = result.First();
//        activeBorrowing.BorrowingRecordId.Should().Be(activeRecord.Id);
//        activeBorrowing.LabAssetId.Should().Be(assetId);
//        activeBorrowing.AssetName.Should().Be("Multimeter");
//        activeBorrowing.SerialNumber.Should().Be("MM-001");
//        activeBorrowing.BorrowedAt.Should().Be(activeRecord.BorrowedAt);
//    }

//    [Fact]
//    public async Task Handle_WithInvalidUser_ShouldThrowArgumentException()
//    {
//        var userId = Guid.NewGuid();

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());

//        var query = new GetActiveBorrowingsForUser.Query(userId);

//        Func<Task> action = async () => await _handler.Handle(query, CancellationToken.None);

//        await action.Should().ThrowAsync<ArgumentException>().WithMessage("User not found.");
//    }

//    [Fact]
//    public async Task Handle_WithNoActiveBorrowings_ShouldReturnEmptyList()
//    {
//        var userId = Guid.NewGuid();
//        var user = new User { Id = userId };

//        var returnedRecord = new BorrowingRecord
//        {
//            Id = Guid.NewGuid(),
//            UserId = userId,
//            LabAsset = new LabAsset { Name = "Resistors" },
//            ReturnedAt = DateTime.UtcNow
//        };

//        _dbContextMock.Setup(db => db.Users).ReturnsDbSet(new List<User> { user });
//        _dbContextMock.Setup(db => db.BorrowingRecords).ReturnsDbSet(new List<BorrowingRecord> { returnedRecord });

//        var query = new GetActiveBorrowingsForUser.Query(userId);

//        var result = await _handler.Handle(query, CancellationToken.None);

//        result.Should().NotBeNull();
//        result.Should().BeEmpty();
//    }
//}