using FluentAssertions;
using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Domain.Exceptions;
using Moq;
using Xunit;

namespace LabResource.Application.UnitTests.Services;

public class LabAssetServiceTests
{
    private readonly Mock<ILabAssetRepository> _labAssetRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LabAssetService _labAssetService;

    public LabAssetServiceTests()
    {
        _labAssetRepositoryMock = new Mock<ILabAssetRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _labAssetService = new LabAssetService(_labAssetRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidDataAndNoTeacher_ShouldCreateAndReturnResult()
    {
        var request = new CreateLabAssetRequest
        {
            Name = "Oscilloscope",
            SerialNumber = "SN-12345",
            Location = "Room A",
            AssignedTeacherId = null
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.CreateAssetAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Oscilloscope");
        result.SerialNumber.Should().Be("SN-12345");
        result.Status.Should().Be(AssetStatus.Available);
        result.IsActive.Should().BeTrue();

        _labAssetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<LabAsset>()), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidTeacher_ShouldCreateAndReturnResult()
    {
        var teacherId = Guid.NewGuid();
        var teacher = new User { Id = teacherId, Role = UserRole.Teacher };
        var request = new CreateLabAssetRequest
        {
            Name = "Oscilloscope",
            SerialNumber = "SN-12345",
            AssignedTeacherId = teacherId
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(teacherId))
            .ReturnsAsync(teacher);
        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync((LabAsset?)null);

        var result = await _labAssetService.CreateAssetAsync(request);

        result.Should().NotBeNull();
        result.AssignedTeacherId.Should().Be(teacherId);
    }

    [Fact]
    public async Task Handle_WithInvalidTeacherId_ShouldThrowNotFoundException()
    {
        var teacherId = Guid.NewGuid();
        var request = new CreateLabAssetRequest { Name = "Osc", AssignedTeacherId = teacherId };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(teacherId))
            .ReturnsAsync((User?)null);

        var act = async () => await _labAssetService.CreateAssetAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonTeacherRole_ShouldThrowBadRequestException()
    {
        var teacherId = Guid.NewGuid();
        var student = new User { Id = teacherId, Role = UserRole.Student };
        var request = new CreateLabAssetRequest { Name = "Osc", AssignedTeacherId = teacherId };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(teacherId))
            .ReturnsAsync(student);

        var act = async () => await _labAssetService.CreateAssetAsync(request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WithExistingSerialNumber_ShouldThrowAlreadyExistsException()
    {
        var existingAsset = new LabAsset { Id = Guid.NewGuid(), SerialNumber = "DUPLICATE-SN" };
        var request = new CreateLabAssetRequest { Name = "New Osc", SerialNumber = "DUPLICATE-SN" };

        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync(request.SerialNumber))
            .ReturnsAsync(existingAsset);

        var act = async () => await _labAssetService.CreateAssetAsync(request);

        await act.Should().ThrowAsync<AlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_WithNullSerialNumber_ShouldCreateAndReturnResult()
    {
        var request = new CreateLabAssetRequest { Name = "Pack of Resistors", SerialNumber = null };

        var result = await _labAssetService.CreateAssetAsync(request);

        result.Should().NotBeNull();
        result.SerialNumber.Should().BeNull();

        _labAssetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<LabAsset>()), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidIdAndActiveBorrowing_ShouldReturnMappedResultWithBorrower()
    {
        var assetId = Guid.NewGuid();
        var borrowDate = DateTime.UtcNow.AddDays(-1);
        var user = new User { Id = Guid.NewGuid(), FullName = "Jane Doe" };
        var activeBorrowing = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            ActualReturnedAt = null,
            Status = BorrowingStatus.Active,
            ActualBorrowedAt = borrowDate,
            User = user
        };

        var asset = new LabAsset
        {
            Id = assetId,
            Name = "Oscilloscope",
            SerialNumber = "OSC-123",
            Status = AssetStatus.Borrowed,
            IsActive = true,
            BorrowingRecords = new List<BorrowingRecord> { activeBorrowing }
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(asset);

        var result = await _labAssetService.GetAssetByIdAsync(assetId);

        result.Should().NotBeNull();
        result.Id.Should().Be(assetId);
        result.Name.Should().Be("Oscilloscope");
        result.Status.Should().Be(AssetStatus.Borrowed);
        result.CurrentBorrowerName.Should().Be("Jane Doe");
        result.CurrentBorrowDate.Should().Be(borrowDate);
    }

    [Fact]
    public async Task Handle_WithValidIdAndNoActiveBorrowing_ShouldReturnResultWithNullBorrower()
    {
        var assetId = Guid.NewGuid();
        var pastBorrowing = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            ActualReturnedAt = DateTime.UtcNow,
            Status = BorrowingStatus.Returned,
            User = new User { FullName = "Old Borrower" }
        };

        var asset = new LabAsset
        {
            Id = assetId,
            Name = "Multimeter",
            Status = AssetStatus.Available,
            IsActive = true,
            BorrowingRecords = new List<BorrowingRecord> { pastBorrowing }
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(asset);

        var result = await _labAssetService.GetAssetByIdAsync(assetId);

        result.Should().NotBeNull();
        result.Id.Should().Be(assetId);
        result.CurrentBorrowerName.Should().BeNull();
        result.CurrentBorrowDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowNotFoundException()
    {
        var assetId = Guid.NewGuid();

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync((LabAsset?)null);

        var act = async () => await _labAssetService.GetAssetByIdAsync(assetId);

        await act.Should().ThrowAsync<NotFoundException>();
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
        }
    };

        _labAssetRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(assets);

        var result = await _labAssetService.GetAllActiveAssetsAsync();

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
        _labAssetRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(new List<LabAsset>());

        var result = await _labAssetService.GetAllActiveAssetsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenDatabaseIsEmpty_ShouldReturnEmptyList()
    {
        _labAssetRepositoryMock.Setup(repo => repo.GetAllActiveAsync())
            .ReturnsAsync(new List<LabAsset>());

        var result = await _labAssetService.GetAllActiveAssetsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAssetAsync_WithValidData_UpdatesAssetSuccessfully()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Old Name", SerialNumber = "OLD-123" };
        var request = new UpdateLabAssetRequest { Name = "New Name", SerialNumber = "NEW-456", Location = "New Location", AssignedTeacherId = null, IsActive = true };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(existingAsset);
        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync("NEW-456")).ReturnsAsync((LabAsset?)null);

        await _labAssetService.UpdateAssetAsync(assetId, request);

        existingAsset.Name.Should().Be("New Name");
        existingAsset.SerialNumber.Should().Be("NEW-456");
        existingAsset.Location.Should().Be("New Location");
        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(existingAsset), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAssetAsync_WithValidTeacherId_UpdatesAssignedTeacher()
    {
        var assetId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var teacher = new User { Id = teacherId, Role = UserRole.Teacher };
        var existingAsset = new LabAsset { Id = assetId, Name = "Asset", SerialNumber = "SN-1" };
        var request = new UpdateLabAssetRequest { Name = "Name", SerialNumber = "SN-1", Location = "Loc", AssignedTeacherId = teacherId, IsActive = true };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(existingAsset);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(teacherId)).ReturnsAsync(teacher);

        await _labAssetService.UpdateAssetAsync(assetId, request);

        existingAsset.AssignedTeacherId.Should().Be(teacherId);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAssetAsync_DuplicateSerialNumber_ThrowsAlreadyExistsException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset { Id = assetId, Name = "Asset 1", SerialNumber = "SN-001" };
        var otherAsset = new LabAsset { Id = Guid.NewGuid(), Name = "Asset 2", SerialNumber = "DUPLICATE" };
        var request = new UpdateLabAssetRequest { Name = "Updated Asset 1", SerialNumber = "DUPLICATE", Location = "Loc", AssignedTeacherId = null, IsActive = true };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(existingAsset);
        _labAssetRepositoryMock.Setup(repo => repo.GetBySerialNumberAsync("DUPLICATE")).ReturnsAsync(otherAsset);

        var act = async () => await _labAssetService.UpdateAssetAsync(assetId, request);

        await act.Should().ThrowAsync<AlreadyExistsException>();
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAssetAsync_NonExistentId_ThrowsNotFoundException()
    {
        var assetId = Guid.NewGuid();
        var request = new UpdateLabAssetRequest { Name = "New Name", SerialNumber = "NEW-123", Location = "Loc", AssignedTeacherId = null, IsActive = true };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync((LabAsset?)null);

        var act = async () => await _labAssetService.UpdateAssetAsync(assetId, request);

        await act.Should().ThrowAsync<NotFoundException>();
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAssetAsync_UserIsNotTeacher_ThrowsBadRequestException()
    {
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var student = new User { Id = userId, Role = UserRole.Student };
        var existingAsset = new LabAsset { Id = assetId, Name = "Asset", SerialNumber = "SN-1" };
        var request = new UpdateLabAssetRequest { Name = "Name", SerialNumber = "SN-1", Location = "Loc", AssignedTeacherId = userId, IsActive = true };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(existingAsset);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(student);

        var act = async () => await _labAssetService.UpdateAssetAsync(assetId, request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task DeactivateAssetAsync_ValidAvailableAsset_DeactivatesSuccessfully()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = true,
            Status = AssetStatus.Available
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        await _labAssetService.DeactivateAssetAsync(assetId);

        existingAsset.IsActive.Should().BeFalse();
        _labAssetRepositoryMock.Verify(repo => repo.UpdateAsync(existingAsset), Times.Once);
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAssetAsync_NonExistentAsset_ThrowsNotFoundException()
    {
        var assetId = Guid.NewGuid();
        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync((LabAsset?)null);

        var act = async () => await _labAssetService.DeactivateAssetAsync(assetId);

        await act.Should().ThrowAsync<NotFoundException>();
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeactivateAssetAsync_AssetAlreadyInactive_ThrowsConflictException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = false,
            Status = AssetStatus.Available
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        var act = async () => await _labAssetService.DeactivateAssetAsync(assetId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This asset is already deactivated.");
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeactivateAssetAsync_AssetIsBorrowed_ThrowsConflictException()
    {
        var assetId = Guid.NewGuid();
        var existingAsset = new LabAsset
        {
            Id = assetId,
            IsActive = true,
            Status = AssetStatus.Borrowed
        };

        _labAssetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId))
            .ReturnsAsync(existingAsset);

        var act = async () => await _labAssetService.DeactivateAssetAsync(assetId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot deactivate an asset that is currently borrowed.");
        _labAssetRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

}