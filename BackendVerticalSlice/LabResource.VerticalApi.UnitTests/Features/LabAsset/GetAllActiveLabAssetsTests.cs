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

public class GetAllActiveLabAssetsTests
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly GetAllActiveLabAssets.Handler _handler;

    public GetAllActiveLabAssetsTests()
    {
        var options = new DbContextOptions<ApplicationDbContext>();
        _dbContextMock = new Mock<ApplicationDbContext>(options);

        _handler = new GetAllActiveLabAssets.Handler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveAssets_AndMapBorrowerAndTeacherCorrectly()
    {
        var teacher = new User { Id = Guid.NewGuid(), FullName = "Dr. Smith" };
        var student = new User { Id = Guid.NewGuid(), FullName = "John Doe" };
        var borrowDate = DateTime.UtcNow.AddDays(-1);

        var activeBorrowing = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            ActualReturnedAt = null,
            ActualBorrowedAt = borrowDate,
            Status = BorrowingStatus.Active,
            User = student
        };

        var pastBorrowing = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            ActualReturnedAt = DateTime.UtcNow.AddDays(-5),
            ActualBorrowedAt = DateTime.UtcNow.AddDays(-10),
            Status = BorrowingStatus.Returned,
            User = new User { FullName = "Old Borrower" }
        };

        var assets = new List<LabAsset>
        {
            new LabAsset
            {
                Id = Guid.NewGuid(),
                Name = "Borrowed Oscilloscope",
                Status = AssetStatus.Borrowed,
                IsActive = true,
                AssignedTeacher = teacher,
                AssignedTeacherId = teacher.Id,
                BorrowingRecords = new List<BorrowingRecord> { pastBorrowing, activeBorrowing }
            },
            new LabAsset
            {
                Id = Guid.NewGuid(),
                Name = "Available Multimeter",
                Status = AssetStatus.Available,
                IsActive = true,
                BorrowingRecords = new List<BorrowingRecord> { pastBorrowing }
            },
            new LabAsset
            {
                Id = Guid.NewGuid(),
                Name = "Broken Cable",
                IsActive = false
            }
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(assets);

        var query = new GetAllActiveLabAssets.Query();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var borrowedAsset = result.First(a => a.Name == "Borrowed Oscilloscope");
        borrowedAsset.CurrentBorrowerName.Should().Be("John Doe");
        borrowedAsset.CurrentBorrowDate.Should().Be(borrowDate);
        borrowedAsset.AssignedTeacherName.Should().Be("Dr. Smith");

        var availableAsset = result.First(a => a.Name == "Available Multimeter");
        availableAsset.CurrentBorrowerName.Should().BeNull();
        availableAsset.CurrentBorrowDate.Should().BeNull();
        availableAsset.AssignedTeacherName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNoActiveAssetsExist_ShouldReturnEmptyList()
    {
        var assets = new List<LabAsset>
        {
            new LabAsset { Id = Guid.NewGuid(), Name = "Asset 1", IsActive = false },
            new LabAsset { Id = Guid.NewGuid(), Name = "Asset 2", IsActive = false }
        };

        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(assets);

        var query = new GetAllActiveLabAssets.Query();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        _dbContextMock.Setup(db => db.LabAssets).ReturnsDbSet(new List<LabAsset>());

        var query = new GetAllActiveLabAssets.Query();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}