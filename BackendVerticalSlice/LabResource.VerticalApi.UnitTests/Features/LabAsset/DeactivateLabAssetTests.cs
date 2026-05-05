using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Exceptions;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.LabAssets;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.LabAssets;

public class DeactivateLabAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly DeactivateLabAsset.Handler _handler;

    public DeactivateLabAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new DeactivateLabAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidIdAndAvailableStatus_ShouldDeactivateAsset()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = true,
            Status = AssetStatus.Available
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new DeactivateLabAsset.Command(assetId);

        await _handler.Handle(command, CancellationToken.None);

        existingAsset.IsActive.Should().BeFalse();
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowNotFoundException()
    {
        var assetId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new DeactivateLabAsset.Command(assetId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadyDeactivated_ShouldThrowConflictException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = false,
            Status = AssetStatus.Available
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new DeactivateLabAsset.Command(assetId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Asset is already deactivated.");
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAssetIsBorrowed_ShouldThrowConflictException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = true,
            Status = AssetStatus.Borrowed
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new DeactivateLabAsset.Command(assetId);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Cannot deactivate a borrowed asset.");
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}