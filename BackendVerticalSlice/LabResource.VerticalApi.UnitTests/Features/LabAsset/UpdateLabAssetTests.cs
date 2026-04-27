//using FluentAssertions;
//using LabResource.VerticalApi.Common.Entities;
//using LabResource.VerticalApi.Common.Persistence;
//using LabResource.VerticalApi.Features.LabAssets;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using Moq.EntityFrameworkCore;
//using Xunit;

//namespace LabResource.VerticalApi.UnitTests.Features.LabAssets;

//public class UpdateLabAssetTests
//{
//    private readonly Mock<ApplicationDbContext> _dbContextMock;
//    private readonly UpdateLabAsset.Handler _handler;

//    public UpdateLabAssetTests()
//    {
//        var options = new DbContextOptions<ApplicationDbContext>();
//        _dbContextMock = new Mock<ApplicationDbContext>(options);

//        _handler = new UpdateLabAsset.Handler(_dbContextMock.Object);
//    }

//    [Fact]
//    public async Task Handle_WithValidData_ShouldUpdateAndReturnTrue()
//    {
//        var assetId = Guid.NewGuid();
//        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "OLD-123" };

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

//        var command = new UpdateLabAsset.Command(assetId, "New Name", "NEW-456");

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeTrue();
//        existingAsset.Name.Should().Be("New Name");
//        existingAsset.SerialNumber.Should().Be("NEW-456");

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithDuplicateSerialNumber_ShouldThrowArgumentException()
//    {
//        var assetId = Guid.NewGuid();
//        var existingAsset = new LabAsset { Id = assetId, Name = "Asset 1", SerialNumber = "SN-001" };
//        var otherAsset = new LabAsset { Id = Guid.NewGuid(), Name = "Asset 2", SerialNumber = "DUPLICATE" };

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset, otherAsset });

//        var command = new UpdateLabAsset.Command(assetId, "Updated Asset 1", "DUPLICATE");

//        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

//        await action.Should().ThrowAsync<ArgumentException>()
//            .WithMessage("An asset with serial number 'DUPLICATE' already exists.");

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }

//    [Fact]
//    public async Task Handle_WithSameSerialNumber_ShouldUpdateSuccessfully()
//    {
//        var assetId = Guid.NewGuid();
//        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "SAME-123" };

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

//        var command = new UpdateLabAsset.Command(assetId, "New Name", "SAME-123");

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeTrue();
//        existingAsset.Name.Should().Be("New Name");
//        existingAsset.SerialNumber.Should().Be("SAME-123");

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WithInvalidId_ShouldReturnFalse()
//    {
//        var assetId = Guid.NewGuid();

//        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

//        var command = new UpdateLabAsset.Command(assetId, "New Name", "NEW-123");

//        var result = await _handler.Handle(command, CancellationToken.None);

//        result.Should().BeFalse();

//        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
//    }
//}