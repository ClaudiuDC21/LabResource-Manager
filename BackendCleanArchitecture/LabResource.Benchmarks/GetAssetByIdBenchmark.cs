using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;

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

    [Benchmark(Baseline = true)]
    public async Task<LabAssetResponse> CleanArchitecture_GetById()
    {
        return await _cleanArchitectureService.GetAssetByIdAsync(_testAssetId);
    }
}