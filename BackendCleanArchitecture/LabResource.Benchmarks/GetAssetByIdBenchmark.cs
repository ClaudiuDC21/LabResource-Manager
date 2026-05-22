using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "CleanArchitecture Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class GetAssetByIdBenchmark
{
    private LabAssetService _cleanArchitectureService;
    private Guid _testAssetId;

    [GlobalSetup]
    public void Setup()
    {
        _testAssetId = Guid.NewGuid();

        var mockAssetRepo = new Mock<ILabAssetRepository>();

        mockAssetRepo
            .Setup(repo => repo.GetByIdAsync(_testAssetId))
            .ReturnsAsync(new LabAsset
            {
                Id = _testAssetId,
                Name = "Digital Oscilloscope",
                SerialNumber = "OSC-2023-XYZ",
                IsActive = true,
                Status = AssetStatus.Available
            });

        var mockUserRepo = new Mock<IUserRepository>();

        _cleanArchitectureService = new LabAssetService(mockAssetRepo.Object, mockUserRepo.Object);
    }

    [Benchmark]
    public async Task<LabAssetResponse> CleanArchitecture_GetById()
    {
        return await _cleanArchitectureService.GetAssetByIdAsync(_testAssetId);
    }
}