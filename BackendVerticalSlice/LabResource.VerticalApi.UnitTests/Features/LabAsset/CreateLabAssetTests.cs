using FluentAssertions;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.LabAssets;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace LabResource.VerticalApi.UnitTests.Features.LabAssets;

public class CreateLabAssetTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly CreateLabAsset.Handler _handler;

    public CreateLabAssetTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new CreateLabAsset.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_WithUniqueSerialNumber_ShouldCreateAndReturnResult()
    {
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new CreateLabAsset.Command("Oscilloscope", "SN-12345");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Oscilloscope");
        result.SerialNumber.Should().Be("SN-12345");
        result.Status.Should().Be(AssetStatus.Available);
        result.IsActive.Should().BeTrue();

        _dbContextMock.Verify(db => db.LabAssets.AddAsync(It.IsAny<LabAsset>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingSerialNumber_ShouldThrowArgumentException()
    {
        var existingAsset = new LabAsset { Id = Guid.NewGuid(), Name = "Old Osc", SerialNumber = "DUPLICATE-SN" };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset> { existingAsset });

        var command = new CreateLabAsset.Command("New Osc", "DUPLICATE-SN");

        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"An asset with serial number 'DUPLICATE-SN' already exists.");

        _dbContextMock.Verify(db => db.LabAssets.AddAsync(It.IsAny<LabAsset>(), It.IsAny<CancellationToken>()), Times.Never);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullOrEmptySerialNumber_ShouldSkipValidationAndCreate()
    {
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var command = new CreateLabAsset.Command("Pack of Resistors", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Pack of Resistors");
        result.SerialNumber.Should().BeNull();

        _dbContextMock.Verify(db => db.LabAssets.AddAsync(It.IsAny<LabAsset>(), It.IsAny<CancellationToken>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}