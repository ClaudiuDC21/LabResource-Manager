using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using LabResource.Infrastructure.Persistence;
using LabResource.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LabResource.Benchmarks;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "CleanArchitecture Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class GetAssetByIdBenchmark
{
    private LabAssetService _cleanArchitectureService = null!;
    private Guid _testAssetId;

    [GlobalSetup]
    public void Setup()
    {
        _testAssetId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        var asset = new LabAsset
        {
            Id = _testAssetId,
            Name = "Digital Oscilloscope",
            SerialNumber = "OSC-2023-XYZ",
            IsActive = true,
            Status = AssetStatus.Available
        };

        context.LabAssets.Add(asset);
        context.SaveChanges();

        ILabAssetRepository assetRepo = new LabAssetRepository(context);
        IUserRepository userRepo = new UserRepository(context);

        _cleanArchitectureService = new LabAssetService(assetRepo, userRepo);
    }

    [Benchmark]
    public async Task<LabAssetResponse?> CleanArchitecture_GetById()
    {
        return await _cleanArchitectureService.GetAssetByIdAsync(_testAssetId);
    }
}