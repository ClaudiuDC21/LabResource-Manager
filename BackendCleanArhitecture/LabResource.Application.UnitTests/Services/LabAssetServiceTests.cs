using FluentAssertions;
using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;
using Xunit;

namespace LabResource.Application.UnitTests.Services;

public class LabAssetServiceTests
{
    private readonly Mock<ILabAssetRepository> _labAssetRepositoryMock;
    private readonly LabAssetService _labAssetService;

    public LabAssetServiceTests()
    {
        _labAssetRepositoryMock = new Mock<ILabAssetRepository>();
        _labAssetService = new LabAssetService(_labAssetRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAssetAsync_WithUniqueSerialNumber_ShouldCreateAndReturnAsset()
    {
        var request = new CreateLabAssetRequest { Name = "Oscilloscope", SerialNumber = "SN12345" };

        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.CreateAssetAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Oscilloscope");
        result.SerialNumber.Should().Be("SN12345");
        result.Status.Should().Be(AssetStatus.Available);
        result.IsActive.Should().BeTrue();

        _labAssetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<LabAsset>()), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAssetAsync_WithExistingSerialNumber_ShouldThrowArgumentException()
    {
        var request = new CreateLabAssetRequest { Name = "Oscilloscope", SerialNumber = "SN12345" };
        var existingAsset = new LabAsset { Id = Guid.NewGuid(), SerialNumber = "SN12345" };

        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync(existingAsset);

        Func<Task> action = async () => await _labAssetService.CreateAssetAsync(request);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage($"An asset with serial number '{request.SerialNumber}' already exists.");
        _labAssetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<LabAsset>()), Times.Never);
    }

    [Fact]
    public async Task CreateAssetAsync_WithNullOrWhitespaceSerialNumber_ShouldNotCheckForDuplicates()
    {
        var request = new CreateLabAssetRequest { Name = "Cables", SerialNumber = " " };

        var result = await _labAssetService.CreateAssetAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Cables");

        _labAssetRepositoryMock.Verify(repo => repo.GetBySerialNumberAsync(It.IsAny<string>()), Times.Never);
        _labAssetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<LabAsset>()), Times.Once);
    }

    [Fact]
    public async Task GetAllActiveAssetsAsync_ShouldReturnMappedAssets()
    {
        var activeBorrowing = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            BorrowedAt = DateTime.UtcNow,
            ReturnedAt = null,
            User = new User { FullName = "John Doe" }
        };

        var assets = new List<LabAsset>
        {
            new LabAsset
            {
                Id = Guid.NewGuid(),
                Name = "Asset 1",
                Status = AssetStatus.Borrowed,
                BorrowingRecords = new List<BorrowingRecord> { activeBorrowing }
            },
            new LabAsset { Id = Guid.NewGuid(), Name = "Asset 2", Status = AssetStatus.Available }
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(assets);

        var result = await _labAssetService.GetAllActiveAssetsAsync();

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstAsset = result.First(a => a.Name == "Asset 1");
        firstAsset.CurrentBorrowerName.Should().Be("John Doe");
        firstAsset.CurrentBorrowDate.Should().NotBeNull();
        firstAsset.CurrentBorrowDate.Should().Be(activeBorrowing.BorrowedAt);
    }

    [Fact]
    public async Task GetAssetByIdAsync_WithValidId_ShouldReturnAsset()
    {
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId, Name = "Asset 1" };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(asset);

        var result = await _labAssetService.GetAssetByIdAsync(assetId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(assetId);
        result.Name.Should().Be("Asset 1");
    }

    [Fact]
    public async Task GetAssetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var assetId = Guid.NewGuid();

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.GetAssetByIdAsync(assetId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAssetAsync_WithValidData_ShouldUpdateAndReturnTrue()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "OLD123" };
        var request = new CreateLabAssetRequest { Name = "New Name", SerialNumber = "NEW123" };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.UpdateAssetAsync(assetId, request);

        result.Should().BeTrue();
        existingAsset.Name.Should().Be("New Name");
        existingAsset.SerialNumber.Should().Be("NEW123");

        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(existingAsset), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAssetAsync_WithDuplicateSerialNumber_ShouldThrowArgumentException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "OLD123" };
        var request = new CreateLabAssetRequest { Name = "New Name", SerialNumber = "DUPLICATE" };
        var anotherAsset = new LabAsset { Id = Guid.NewGuid(), SerialNumber = "DUPLICATE" };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync(anotherAsset);

        Func<Task> action = async () => await _labAssetService.UpdateAssetAsync(assetId, request);

        await action.Should().ThrowAsync<ArgumentException>().WithMessage($"An asset with serial number '{request.SerialNumber}' already exists.");
        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<LabAsset>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAssetAsync_WithSameSerialNumber_ShouldNotThrowException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "SAME123" };
        var request = new CreateLabAssetRequest { Name = "New Name", SerialNumber = "SAME123" };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        var result = await _labAssetService.UpdateAssetAsync(assetId, request);

        result.Should().BeTrue();
        existingAsset.Name.Should().Be("New Name");

        _labAssetRepositoryMock.Verify(repo => repo.GetBySerialNumberAsync(It.IsAny<string>()), Times.Never);
        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(existingAsset), Times.Once);
    }

    [Fact]
    public async Task UpdateAssetAsync_WithInvalidId_ShouldReturnFalse()
    {
        var assetId = Guid.NewGuid();
        var request = new CreateLabAssetRequest { Name = "New Name" };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.UpdateAssetAsync(assetId, request);

        result.Should().BeFalse();
        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<LabAsset>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateAssetAsync_WithValidId_ShouldSetIsActiveToFalseAndReturnTrue()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, IsActive = true };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        var result = await _labAssetService.DeactivateAssetAsync(assetId);

        result.Should().BeTrue();
        existingAsset.IsActive.Should().BeFalse();

        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(existingAsset), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAssetAsync_WithInvalidId_ShouldReturnFalse()
    {
        var assetId = Guid.NewGuid();

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.DeactivateAssetAsync(assetId);

        result.Should().BeFalse();
        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<LabAsset>()), Times.Never);
    }
}