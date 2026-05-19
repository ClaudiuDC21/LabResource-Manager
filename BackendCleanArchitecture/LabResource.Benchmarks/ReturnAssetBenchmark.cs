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

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "ThesisJob")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class ReturnAssetBenchmark
{
    private BorrowingService _cleanArchitectureService;
    private ReturnAssetRequest _request;
    private Guid _borrowingId;
    private Guid _assetId;

    [GlobalSetup]
    public void Setup()
    {
        _borrowingId = Guid.NewGuid();
        _assetId = Guid.NewGuid();

        _request = new ReturnAssetRequest
        {
            Remarks = "Returned in good condition.",
            IsDefective = false
        };

        var mockBorrowingRepo = new Mock<IBorrowingRecordRepository>();
        mockBorrowingRepo.Setup(repo => repo.GetByIdAsync(_borrowingId))
            .ReturnsAsync(new BorrowingRecord
            {
                Id = _borrowingId,
                LabAssetId = _assetId,
                Status = BorrowingStatus.Active,
                Remarks = "Picked up on Monday for field research."
            });

        var mockAssetRepo = new Mock<ILabAssetRepository>();
        mockAssetRepo.Setup(repo => repo.GetByIdAsync(_assetId))
            .ReturnsAsync(new LabAsset
            {
                Id = _assetId,
                Status = AssetStatus.Borrowed,
                Name = "Thermal Camera"
            });

        var mockUserRepo = new Mock<IUserRepository>();

        _cleanArchitectureService = new BorrowingService(mockUserRepo.Object, mockAssetRepo.Object, mockBorrowingRepo.Object);
    }

    [Benchmark(Baseline = true)]
    public async Task<ReturnAssetResponse> CleanArchitecture_ReturnAsset()
    {
        return await _cleanArchitectureService.ReturnAssetAsync(_borrowingId, _request);
    }
}