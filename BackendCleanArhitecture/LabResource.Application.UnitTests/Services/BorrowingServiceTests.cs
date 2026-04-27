using FluentAssertions;
using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;
using Xunit;

namespace LabResource.Application.UnitTests.Services;

public class BorrowingServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILabAssetRepository> _assetRepositoryMock;
    private readonly Mock<IBorrowingRecordRepository> _borrowingRepositoryMock;
    private readonly BorrowingService _borrowingService;

    public BorrowingServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _assetRepositoryMock = new Mock<ILabAssetRepository>();
        _borrowingRepositoryMock = new Mock<IBorrowingRecordRepository>();

        _borrowingService = new BorrowingService(
            _userRepositoryMock.Object,
            _assetRepositoryMock.Object,
            _borrowingRepositoryMock.Object);
    }

    //[Fact]
    //public async Task BorrowAssetAsync_WithValidData_ShouldCreateBorrowingAndReturnResponse()
    //{
    //    var request = new BorrowAssetRequest { UserId = Guid.NewGuid(), LabAssetId = Guid.NewGuid() };
    //    var user = new User { Id = request.UserId, FullName = "John Doe", IsActive = true };
    //    var asset = new LabAsset { Id = request.LabAssetId, Name = "Microscope", Status = AssetStatus.Available, IsActive = true };

    //    _userRepositoryMock.Setup(repo => repo.GetByIdAsync(request.UserId))
    //        .ReturnsAsync(user);
    //    _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(request.LabAssetId))
    //        .ReturnsAsync(asset);

    //    var result = await _borrowingService.BorrowAssetAsync(request);

    //    result.Should().NotBeNull();
    //    result.UserId.Should().Be(request.UserId);
    //    result.LabAssetId.Should().Be(request.LabAssetId);
    //    result.UserName.Should().Be("John Doe");
    //    result.AssetName.Should().Be("Microscope");

    //    asset.Status.Should().Be(AssetStatus.Borrowed);

    //    _borrowingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<BorrowingRecord>()), Times.Once);
    //    _assetRepositoryMock.Verify(repo => repo.UpdateAsync(asset), Times.Once);
    //    _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    //}

    //[Fact]
    //public async Task BorrowAssetAsync_WithInactiveUser_ShouldThrowArgumentException()
    //{
    //    var request = new BorrowAssetRequest { UserId = Guid.NewGuid(), LabAssetId = Guid.NewGuid() };
    //    var user = new User { Id = request.UserId, IsActive = false };

    //    _userRepositoryMock.Setup(repo => repo.GetByIdAsync(request.UserId))
    //        .ReturnsAsync(user);

    //    Func<Task> action = async () => await _borrowingService.BorrowAssetAsync(request);

    //    await action.Should().ThrowAsync<ArgumentException>().WithMessage("User not found or inactive.");
    //    _borrowingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<BorrowingRecord>()), Times.Never);
    //}

    [Fact]
    public async Task ReturnAssetAsync_WhenNotDefective_ShouldUpdateStatusToAvailable()
    {
        var request = new ReturnAssetRequest { LabAssetId = Guid.NewGuid(), Remarks = "All good", IsDefective = false };
        var activeBorrowing = new BorrowingRecord { Id = Guid.NewGuid(), LabAssetId = request.LabAssetId, ActualReturnedAt = null };
        var asset = new LabAsset { Id = request.LabAssetId, Name = "Microscope", Status = AssetStatus.Borrowed };

        _borrowingRepositoryMock.Setup(repo => repo.GetActiveBorrowingByAssetIdAsync(request.LabAssetId))
            .ReturnsAsync(activeBorrowing);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(request.LabAssetId))
            .ReturnsAsync(asset);

        var result = await _borrowingService.ReturnAssetAsync(request);

        result.Should().NotBeNull();
        result.NewStatus.Should().Be(AssetStatus.Available);
        result.ReturnedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        activeBorrowing.Remarks.Should().Be("All good");
        asset.Status.Should().Be(AssetStatus.Available);

        _borrowingRepositoryMock.Verify(repo => repo.UpdateAsync(activeBorrowing), Times.Once);
        _assetRepositoryMock.Verify(repo => repo.UpdateAsync(asset), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReturnAssetAsync_WhenDefective_ShouldUpdateStatusToDefective()
    {
        var request = new ReturnAssetRequest { LabAssetId = Guid.NewGuid(), Remarks = "Broken lens", IsDefective = true };
        var activeBorrowing = new BorrowingRecord { Id = Guid.NewGuid(), LabAssetId = request.LabAssetId, ActualReturnedAt = null };
        var asset = new LabAsset { Id = request.LabAssetId, Name = "Microscope", Status = AssetStatus.Borrowed };

        _borrowingRepositoryMock.Setup(repo => repo.GetActiveBorrowingByAssetIdAsync(request.LabAssetId))
            .ReturnsAsync(activeBorrowing);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(request.LabAssetId))
            .ReturnsAsync(asset);

        var result = await _borrowingService.ReturnAssetAsync(request);

        result.Should().NotBeNull();
        result.NewStatus.Should().Be(AssetStatus.Defective);
        asset.Status.Should().Be(AssetStatus.Defective);
    }

    [Fact]
    public async Task ReturnAssetAsync_WithNoActiveBorrowing_ShouldThrowInvalidOperationException()
    {
        var request = new ReturnAssetRequest { LabAssetId = Guid.NewGuid() };

        _borrowingRepositoryMock.Setup(repo => repo.GetActiveBorrowingByAssetIdAsync(request.LabAssetId))
            .ReturnsAsync((BorrowingRecord?)null);

        Func<Task> action = async () => await _borrowingService.ReturnAssetAsync(request);

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("No active borrowing record found for this asset.");
    }

    [Fact]
    public async Task GetActiveBorrowingsForUserAsync_WithValidUser_ShouldReturnMappedBorrowings()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        var activeBorrowings = new List<BorrowingRecord>
        {
            new BorrowingRecord
            {
                Id = Guid.NewGuid(),
                LabAssetId = Guid.NewGuid(),
                ActualBorrowedAt = DateTime.UtcNow.AddDays(-1),
                LabAsset = new LabAsset { Name = "Multimeter", SerialNumber = "M-101" }
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _borrowingRepositoryMock.Setup(repo => repo.GetActiveBorrowingsByUserIdAsync(userId))
            .ReturnsAsync(activeBorrowings);

        var result = await _borrowingService.GetActiveBorrowingsForUserAsync(userId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().AssetName.Should().Be("Multimeter");
        result.First().SerialNumber.Should().Be("M-101");
    }

    [Fact]
    public async Task GetActiveBorrowingsForUserAsync_WithInvalidUser_ShouldThrowArgumentException()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        Func<Task> action = async () => await _borrowingService.GetActiveBorrowingsForUserAsync(userId);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task GetAssetHistoryAsync_WithValidAsset_ShouldReturnMappedHistory()
    {
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId };

        var history = new List<BorrowingRecord>
        {
            new BorrowingRecord
            {
                Id = Guid.NewGuid(),
                User = new User { FullName = "Jane Doe", MatriculationNumber = "12345" },
                ActualBorrowedAt = DateTime.UtcNow.AddDays(-5),
                ActualReturnedAt = DateTime.UtcNow.AddDays(-1),
                Remarks = "Returned clean"
            }
        };

        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(asset);
        _borrowingRepositoryMock.Setup(repo => repo.GetHistoryByAssetIdAsync(assetId))
            .ReturnsAsync(history);

        var result = await _borrowingService.GetAssetHistoryAsync(assetId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().UserName.Should().Be("Jane Doe");
        result.First().Remarks.Should().Be("Returned clean");
    }
}