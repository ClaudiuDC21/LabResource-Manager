using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.Borrowings;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "CleanArchitecture Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class RequestAssetBenchmark
{
    private BorrowingService _cleanArchitectureService;
    private BorrowAssetRequest _request;
    private Guid _userId;
    private Guid _assetId;

    [GlobalSetup]
    public void Setup()
    {
        _userId = Guid.NewGuid();
        _assetId = Guid.NewGuid();

        _request = new BorrowAssetRequest
        {
            UserId = _userId,
            LabAssetId = _assetId,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(3)
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo.Setup(repo => repo.GetByIdAsync(_userId))
            .ReturnsAsync(new User
            {
                Id = _userId,
                IsActive = true,
                FullName = "Emily Chen",
                Email = "emily.chen@stud.ubbcluj.ro"
            });

        var mockAssetRepo = new Mock<ILabAssetRepository>();
        mockAssetRepo.Setup(repo => repo.GetByIdAsync(_assetId))
            .ReturnsAsync(new LabAsset
            {
                Id = _assetId,
                IsActive = true,
                Status = AssetStatus.Available,
                AssignedTeacherId = Guid.NewGuid(),
                Name = "Mass Spectrometer"
            });

        var mockBorrowingRepo = new Mock<IBorrowingRecordRepository>();
        mockBorrowingRepo.Setup(repo => repo.HasOverlappingReservationsAsync(_assetId, _request.RequestedStartDate, _request.RequestedEndDate))
            .ReturnsAsync(false);

        _cleanArchitectureService = new BorrowingService(mockUserRepo.Object, mockAssetRepo.Object, mockBorrowingRepo.Object);
    }

    [Benchmark]
    public async Task<BorrowingResponse> CleanArchitecture_RequestAsset()
    {
        return await _cleanArchitectureService.RequestAssetAsync(_request);
    }
}