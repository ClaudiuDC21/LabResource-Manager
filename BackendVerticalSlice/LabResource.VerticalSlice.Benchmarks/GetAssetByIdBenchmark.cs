using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.LabAssets;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalSlice.Benchmarks;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "VerticalSlice Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class GetAssetByIdBenchmark
{
    private ApplicationDbContext _context = null!;
    private GetLabAssetById.Handler _handler = null!;
    private GetLabAssetById.Query _query = null!;
    private Guid _testAssetId;

    [GlobalSetup]
    public void Setup()
    {
        _testAssetId = Guid.NewGuid();
        _query = new GetLabAssetById.Query(_testAssetId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var asset = new LabAsset
        {
            Id = _testAssetId,
            Name = "Digital Oscilloscope",
            SerialNumber = "OSC-2023-XYZ",
            IsActive = true,
            Status = AssetStatus.Available
        };

        _context.LabAssets.Add(asset);
        _context.SaveChanges();

        _handler = new GetLabAssetById.Handler(_context);
    }

    [Benchmark]
    public async Task<GetLabAssetById.Result> VerticalSlice_GetById()
    {
        return await _handler.Handle(_query, CancellationToken.None);
    }
}