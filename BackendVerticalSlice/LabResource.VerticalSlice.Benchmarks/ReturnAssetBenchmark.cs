using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Borrowings;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalSlice.Benchmarks;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "VerticalSlice Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class ReturnAssetBenchmark
{
    private ApplicationDbContext _context = null!;
    private ReturnAsset.Handler _handler = null!;
    private ReturnAsset.Command _command = null!;
    private Guid _borrowingId;
    private Guid _assetId;

    [IterationSetup]
    public void IterationSetup()
    {
        _borrowingId = Guid.NewGuid();
        _assetId = Guid.NewGuid();

        _command = new ReturnAsset.Command(_borrowingId, "Returnat in stare buna", false);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.LabAssets.Add(new LabAsset { Id = _assetId, Status = AssetStatus.Borrowed, Name = "Thermal Camera" });
        _context.BorrowingRecords.Add(new BorrowingRecord { Id = _borrowingId, LabAssetId = _assetId, Status = BorrowingStatus.Active, Remarks = "Picked up on Monday for field research." });
        _context.SaveChanges();

        _handler = new ReturnAsset.Handler(_context);
    }

    [Benchmark]
    public async Task<ReturnAsset.Result> VerticalSlice_ReturnAsset()
    {
        return await _handler.Handle(_command, CancellationToken.None);
    }
}