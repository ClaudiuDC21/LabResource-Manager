using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
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
    public async Task Handle_WithValidId_ShouldDeactivateAssetAndReturnTrue()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = true
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new DeactivateLabAsset.Command(assetId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        existingAsset.IsActive.Should().BeFalse();

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldReturnFalseAndNotSave()
    {
        var assetId = Guid.NewGuid();

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new DeactivateLabAsset.Command(assetId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();

        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}