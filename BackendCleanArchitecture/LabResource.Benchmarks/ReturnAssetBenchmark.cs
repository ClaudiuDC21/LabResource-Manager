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

namespace LabResource.Benchmarks;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "CleanArchitecture Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class ReturnAssetBenchmark
{
    private BorrowingService _cleanArchitectureService = null!;
    private ReturnAssetRequest _request = null!;
    private Guid _borrowingId;
    private Guid _assetId;

    private Mock<IBorrowingRecordRepository> _mockBorrowingRepo = null!;
    private Mock<ILabAssetRepository> _mockAssetRepo = null!;
    private Mock<IUserRepository> _mockUserRepo = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mockBorrowingRepo = new Mock<IBorrowingRecordRepository>();
        _mockAssetRepo = new Mock<ILabAssetRepository>();
        _mockUserRepo = new Mock<IUserRepository>();

        _cleanArchitectureService = new BorrowingService(
            _mockUserRepo.Object,
            _mockAssetRepo.Object,
            _mockBorrowingRepo.Object
        );
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _borrowingId = Guid.NewGuid();
        _assetId = Guid.NewGuid();

        _request = new ReturnAssetRequest
        {
            Remarks = "Returned in good condition.",
            IsDefective = false
        };

        _mockBorrowingRepo.Setup(repo => repo.GetByIdAsync(_borrowingId))
            .ReturnsAsync(new BorrowingRecord
            {
                Id = _borrowingId,
                LabAssetId = _assetId,
                Status = BorrowingStatus.Active,
                Remarks = "Picked up on Monday."
            });

        _mockAssetRepo.Setup(repo => repo.GetByIdAsync(_assetId))
            .ReturnsAsync(new LabAsset
            {
                Id = _assetId,
                Status = AssetStatus.Borrowed,
                Name = "Thermal Camera"
            });
    }

    [Benchmark]
    public async Task<ReturnAssetResponse> CleanArchitecture_ReturnAsset()
    {
        return await _cleanArchitectureService.ReturnAssetAsync(_borrowingId, _request);
    }
}