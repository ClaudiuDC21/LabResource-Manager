using FluentAssertions;
using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Domain.Exceptions;
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

    [Fact]
    public async Task RequestAssetAsync_ValidData_CreatesPendingRequest()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var request = new BorrowAssetRequest
        {
            UserId = userId,
            LabAssetId = assetId,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(2)
        };

        var user = new User { Id = userId, FullName = "John Doe", IsActive = true };
        var asset = new LabAsset { Id = assetId, Name = "Oscilloscope", Status = AssetStatus.Available, IsActive = true, AssignedTeacherId = Guid.NewGuid() };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);
        _borrowingRepositoryMock.Setup(repo => repo.HasOverlappingReservationsAsync(assetId, request.RequestedStartDate, request.RequestedEndDate))
            .ReturnsAsync(false);

        var result = await _borrowingService.RequestAssetAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be(BorrowingStatus.Pending);
        asset.Status.Should().Be(AssetStatus.PendingApproval);
        _borrowingRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<BorrowingRecord>()), Times.Once);
        _assetRepositoryMock.Verify(repo => repo.UpdateAsync(asset), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RequestAssetAsync_UserIsAssignedTeacher_CreatesApprovedRequest()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var request = new BorrowAssetRequest
        {
            UserId = userId,
            LabAssetId = assetId,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(2)
        };

        var user = new User { Id = userId, FullName = "Prof Smith", IsActive = true };
        var asset = new LabAsset { Id = assetId, Name = "Oscilloscope", Status = AssetStatus.Available, IsActive = true, AssignedTeacherId = userId };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);
        _borrowingRepositoryMock.Setup(repo => repo.HasOverlappingReservationsAsync(assetId, request.RequestedStartDate, request.RequestedEndDate))
            .ReturnsAsync(false);

        var result = await _borrowingService.RequestAssetAsync(request);

        result.Status.Should().Be(BorrowingStatus.Approved);
        asset.Status.Should().Be(AssetStatus.Borrowed);
    }

    [Fact]
    public async Task RequestAssetAsync_InactiveUser_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = false };
        var request = new BorrowAssetRequest { UserId = userId, LabAssetId = Guid.NewGuid() };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = async () => await _borrowingService.RequestAssetAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RequestAssetAsync_DefectiveAsset_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        var asset = new LabAsset { Id = assetId, Status = AssetStatus.Defective, IsActive = true };
        var request = new BorrowAssetRequest { UserId = userId, LabAssetId = assetId };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var act = async () => await _borrowingService.RequestAssetAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RequestAssetAsync_OverlappingBooking_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var request = new BorrowAssetRequest
        {
            UserId = userId,
            LabAssetId = assetId,
            RequestedStartDate = DateTime.UtcNow.AddDays(2),
            RequestedEndDate = DateTime.UtcNow.AddDays(3)
        };

        var user = new User { Id = userId, IsActive = true };
        var asset = new LabAsset { Id = assetId, Status = AssetStatus.Available, IsActive = true };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);
        _borrowingRepositoryMock.Setup(repo => repo.HasOverlappingReservationsAsync(assetId, request.RequestedStartDate, request.RequestedEndDate))
            .ReturnsAsync(true);

        var act = async () => await _borrowingService.RequestAssetAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task PickUpAssetAsync_WithValidApprovedBorrowing_ShouldSetActiveStatus()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Approved
        };
        var asset = new LabAsset
        {
            Id = assetId,
            Status = AssetStatus.Available
        };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        await _borrowingService.PickUpAssetAsync(borrowingId);

        record.Status.Should().Be(BorrowingStatus.Active);
        record.ActualBorrowedAt.Should().NotBeNull();
        asset.Status.Should().Be(AssetStatus.Borrowed);
        _borrowingRepositoryMock.Verify(repo => repo.UpdateAsync(record), Times.Once);
        _assetRepositoryMock.Verify(repo => repo.UpdateAsync(asset), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PickUpAssetAsync_WithInvalidBorrowingId_ShouldThrowNotFoundException()
    {
        var borrowingId = Guid.NewGuid();
        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync((BorrowingRecord?)null);

        var act = async () => await _borrowingService.PickUpAssetAsync(borrowingId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PickUpAssetAsync_WhenBorrowingIsNotApproved_ShouldThrowConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            Status = BorrowingStatus.Pending
        };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);

        var act = async () => await _borrowingService.PickUpAssetAsync(borrowingId);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task PickUpAssetAsync_WhenAssetIsNotAvailable_ShouldThrowConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Approved
        };
        var asset = new LabAsset
        {
            Id = assetId,
            Status = AssetStatus.Defective
        };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var act = async () => await _borrowingService.PickUpAssetAsync(borrowingId);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task ReviewRequestAsync_ValidApproval_SetsApprovedStatus()
    {
        var borrowingId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Pending
        };
        var asset = new LabAsset
        {
            Id = assetId,
            AssignedTeacherId = teacherId
        };
        var request = new ReviewBorrowingRequest { IsApproved = true, TeacherNotes = "Approved notes" };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        await _borrowingService.ReviewRequestAsync(borrowingId, teacherId, request);

        record.Status.Should().Be(BorrowingStatus.Approved);
        record.Remarks.Should().Be("Approved notes");
        _borrowingRepositoryMock.Verify(repo => repo.UpdateAsync(record), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReviewRequestAsync_ValidRejection_SetsRejectedStatusAndMakesAssetAvailable()
    {
        var borrowingId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Pending
        };
        var asset = new LabAsset
        {
            Id = assetId,
            AssignedTeacherId = teacherId,
            Status = AssetStatus.PendingApproval
        };
        var request = new ReviewBorrowingRequest { IsApproved = false, TeacherNotes = "Rejected notes" };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        await _borrowingService.ReviewRequestAsync(borrowingId, teacherId, request);

        record.Status.Should().Be(BorrowingStatus.Rejected);
        record.Remarks.Should().Be("Rejected notes");
        asset.Status.Should().Be(AssetStatus.Available);
        _assetRepositoryMock.Verify(repo => repo.UpdateAsync(asset), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.UpdateAsync(record), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReviewRequestAsync_InvalidBorrowingId_ThrowsNotFoundException()
    {
        var borrowingId = Guid.NewGuid();
        var request = new ReviewBorrowingRequest { IsApproved = true };
        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync((BorrowingRecord?)null);

        var act = async () => await _borrowingService.ReviewRequestAsync(borrowingId, Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ReviewRequestAsync_RequestNotPending_ThrowsConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            Status = BorrowingStatus.Approved
        };
        var request = new ReviewBorrowingRequest { IsApproved = true };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);

        var act = async () => await _borrowingService.ReviewRequestAsync(borrowingId, Guid.NewGuid(), request);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Only pending requests can be reviewed.");
    }

    [Fact]
    public async Task ReviewRequestAsync_UnauthorizedTeacher_ThrowsForbiddenAccessException()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var realTeacherId = Guid.NewGuid();
        var wrongTeacherId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Pending
        };
        var asset = new LabAsset
        {
            Id = assetId,
            AssignedTeacherId = realTeacherId
        };
        var request = new ReviewBorrowingRequest { IsApproved = true };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var act = async () => await _borrowingService.ReviewRequestAsync(borrowingId, wrongTeacherId, request);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task ReturnAssetAsync_ValidActiveBorrowing_ReturnsResultAndUpdatesStatus()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var record = new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            Status = BorrowingStatus.Active,
            Remarks = "Initial remark"
        };
        var asset = new LabAsset
        {
            Id = assetId,
            Name = "Microscope",
            Status = AssetStatus.Borrowed
        };
        var request = new ReturnAssetRequest { Remarks = "Returned in good condition", IsDefective = false };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var result = await _borrowingService.ReturnAssetAsync(borrowingId, request);

        result.Should().NotBeNull();
        result.NewStatus.Should().Be(AssetStatus.Available);
        record.Status.Should().Be(BorrowingStatus.Returned);
        record.ActualReturnedAt.Should().NotBeNull();
        record.Remarks.Should().Contain("Return Note: Returned in good condition");
        asset.Status.Should().Be(AssetStatus.Available);
        _borrowingRepositoryMock.Verify(repo => repo.UpdateAsync(record), Times.Once);
        _assetRepositoryMock.Verify(repo => repo.UpdateAsync(asset), Times.Once);
        _borrowingRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ReturnAssetAsync_AssetIsDefective_SetsStatusToDefective()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var record = new BorrowingRecord { Id = borrowingId, LabAssetId = assetId, Status = BorrowingStatus.Active };
        var asset = new LabAsset { Id = assetId, Name = "Microscope", Status = AssetStatus.Borrowed };
        var request = new ReturnAssetRequest { Remarks = "Broken lens", IsDefective = true };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);

        var result = await _borrowingService.ReturnAssetAsync(borrowingId, request);

        result.NewStatus.Should().Be(AssetStatus.Defective);
        asset.Status.Should().Be(AssetStatus.Defective);
    }

    [Fact]
    public async Task ReturnAssetAsync_NonExistentBorrowingId_ThrowsNotFoundException()
    {
        var borrowingId = Guid.NewGuid();
        var request = new ReturnAssetRequest { Remarks = null, IsDefective = false };
        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync((BorrowingRecord?)null);

        var act = async () => await _borrowingService.ReturnAssetAsync(borrowingId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ReturnAssetAsync_BorrowingNotActive_ThrowsConflictException()
    {
        var borrowingId = Guid.NewGuid();
        var record = new BorrowingRecord { Id = borrowingId, Status = BorrowingStatus.Returned };
        var request = new ReturnAssetRequest { Remarks = null, IsDefective = false };

        _borrowingRepositoryMock.Setup(repo => repo.GetByIdAsync(borrowingId)).ReturnsAsync(record);

        var act = async () => await _borrowingService.ReturnAssetAsync(borrowingId, request);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Cannot return an asset that is not currently active.");
    }

    [Fact]
    public async Task GetUserHistoryAsync_ValidUserWithRecords_ReturnsOrderedHistory()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var asset = new LabAsset { Id = Guid.NewGuid(), Name = "Asset 1", SerialNumber = "SN1", Status = AssetStatus.Available };

        var record1 = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LabAssetId = asset.Id,
            LabAsset = asset,
            RequestedStartDate = DateTime.UtcNow.AddDays(-10),
            RequestedEndDate = DateTime.UtcNow.AddDays(-5),
            ActualReturnedAt = DateTime.UtcNow.AddDays(-5),
            Status = BorrowingStatus.Returned,
            Remarks = "All good"
        };

        var record2 = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LabAssetId = asset.Id,
            LabAsset = asset,
            RequestedStartDate = DateTime.UtcNow.AddDays(-2),
            RequestedEndDate = DateTime.UtcNow.AddDays(2),
            ActualReturnedAt = null,
            Status = BorrowingStatus.Active,
            Remarks = null
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _borrowingRepositoryMock.Setup(repo => repo.GetHistoryByUserIdAsync(userId))
            .ReturnsAsync(new List<BorrowingRecord> { record2, record1 });

        var result = await _borrowingService.GetUserHistoryAsync(userId);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First();
        firstResult.AssetName.Should().Be("Asset 1");
        firstResult.Status.Should().Be(BorrowingStatus.Active);

        var secondResult = result.Last();
        secondResult.Remarks.Should().Be("All good");
    }

    [Fact]
    public async Task GetUserHistoryAsync_NonExistentUser_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var act = async () => await _borrowingService.GetUserHistoryAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetUserHistoryAsync_UserWithNoHistory_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _borrowingRepositoryMock.Setup(repo => repo.GetHistoryByUserIdAsync(userId))
            .ReturnsAsync(new List<BorrowingRecord>());

        var result = await _borrowingService.GetUserHistoryAsync(userId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRequestsForTeacherAsync_WithPendingRequests_ReturnsOnlyRequestsForSpecificTeacher()
    {
        var teacherId = Guid.NewGuid();
        var student = new User { Id = Guid.NewGuid(), FullName = "Student Name" };

        var assetForTeacher = new LabAsset
        {
            Id = Guid.NewGuid(),
            Name = "Oscilloscope",
            AssignedTeacherId = teacherId
        };

        var pendingRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetForTeacher.Id,
            LabAsset = assetForTeacher,
            UserId = student.Id,
            User = student,
            Status = BorrowingStatus.Pending,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(2)
        };

        _borrowingRepositoryMock.Setup(repo => repo.GetPendingRequestsForTeacherAsync(teacherId))
            .ReturnsAsync(new List<BorrowingRecord> { pendingRecord });

        var result = await _borrowingService.GetPendingRequestsForTeacherAsync(teacherId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().BorrowingRecordId.Should().Be(pendingRecord.Id);
        result.First().AssetName.Should().Be("Oscilloscope");
        result.First().UserName.Should().Be("Student Name");
    }

    [Fact]
    public async Task GetPendingRequestsForTeacherAsync_NoPendingRequests_ReturnsEmptyList()
    {
        var teacherId = Guid.NewGuid();

        _borrowingRepositoryMock.Setup(repo => repo.GetPendingRequestsForTeacherAsync(teacherId))
            .ReturnsAsync(new List<BorrowingRecord>());

        var result = await _borrowingService.GetPendingRequestsForTeacherAsync(teacherId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingRequestsForTeacherAsync_DatabaseEmpty_ReturnsEmptyList()
    {
        var teacherId = Guid.NewGuid();

        _borrowingRepositoryMock.Setup(repo => repo.GetPendingRequestsForTeacherAsync(teacherId))
            .ReturnsAsync(new List<BorrowingRecord>());

        var result = await _borrowingService.GetPendingRequestsForTeacherAsync(teacherId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAssetHistoryAsync_AssetWithRecords_ReturnsOrderedHistory()
    {
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId };
        var user = new User { Id = Guid.NewGuid(), FullName = "Alice Smith", MatriculationNumber = "ALICE123" };

        var olderRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetId,
            UserId = user.Id,
            User = user,
            RequestedStartDate = DateTime.UtcNow.AddDays(-10),
            RequestedEndDate = DateTime.UtcNow.AddDays(-8),
            ActualReturnedAt = DateTime.UtcNow.AddDays(-8),
            Status = BorrowingStatus.Returned,
            Remarks = "Returned safely"
        };

        var newerRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            LabAssetId = assetId,
            UserId = user.Id,
            User = user,
            RequestedStartDate = DateTime.UtcNow.AddDays(-2),
            RequestedEndDate = DateTime.UtcNow.AddDays(2),
            ActualReturnedAt = null,
            Status = BorrowingStatus.Active,
            Remarks = null
        };

        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);
        _borrowingRepositoryMock.Setup(repo => repo.GetHistoryByAssetIdAsync(assetId))
            .ReturnsAsync(new List<BorrowingRecord> { newerRecord, olderRecord });

        var result = await _borrowingService.GetAssetHistoryAsync(assetId);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First();
        firstResult.BorrowingRecordId.Should().Be(newerRecord.Id);
        firstResult.UserName.Should().Be("Alice Smith");

        var secondResult = result.Last();
        secondResult.Remarks.Should().Be("Returned safely");
    }

    [Fact]
    public async Task GetAssetHistoryAsync_NonExistentAsset_ThrowsNotFoundException()
    {
        var assetId = Guid.NewGuid();
        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync((LabAsset?)null);

        var act = async () => await _borrowingService.GetAssetHistoryAsync(assetId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAssetHistoryAsync_AssetWithNoHistory_ReturnsEmptyList()
    {
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId };

        _assetRepositoryMock.Setup(repo => repo.GetByIdAsync(assetId)).ReturnsAsync(asset);
        _borrowingRepositoryMock.Setup(repo => repo.GetHistoryByAssetIdAsync(assetId))
            .ReturnsAsync(new List<BorrowingRecord>());

        var result = await _borrowingService.GetAssetHistoryAsync(assetId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBorrowingsForUserAsync_ValidUserWithActiveBorrowings_ReturnsMappedActiveAndApprovedBorrowings()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Test User" };
        var assetId = Guid.NewGuid();
        var asset = new LabAsset { Id = assetId, Name = "Multimeter", SerialNumber = "MM-001" };

        var activeRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            LabAssetId = assetId,
            LabAsset = asset,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(3),
            Status = BorrowingStatus.Active,
            ActualReturnedAt = null
        };

        var approvedRecord = new BorrowingRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            User = user,
            LabAssetId = Guid.NewGuid(),
            LabAsset = new LabAsset { Name = "Oscilloscope" },
            Status = BorrowingStatus.Approved,
            ActualReturnedAt = null
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _borrowingRepositoryMock.Setup(repo => repo.GetActiveBorrowingsByUserIdAsync(userId))
            .ReturnsAsync(new List<BorrowingRecord> { activeRecord, approvedRecord });

        var result = await _borrowingService.GetActiveBorrowingsForUserAsync(userId);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.BorrowingRecordId == activeRecord.Id);
        result.Should().Contain(r => r.BorrowingRecordId == approvedRecord.Id);
        result.Should().NotContain(r => r.Status == BorrowingStatus.Returned);
    }

    [Fact]
    public async Task GetActiveBorrowingsForUserAsync_NonExistentUser_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var act = async () => await _borrowingService.GetActiveBorrowingsForUserAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetActiveBorrowingsForUserAsync_UserWithNoActiveBorrowings_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _borrowingRepositoryMock.Setup(repo => repo.GetActiveBorrowingsByUserIdAsync(userId))
            .ReturnsAsync(new List<BorrowingRecord>());

        var result = await _borrowingService.GetActiveBorrowingsForUserAsync(userId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}