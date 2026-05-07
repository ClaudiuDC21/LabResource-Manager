using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Borrowings;
using Microsoft.EntityFrameworkCore;

[MemoryDiagnoser]
public class ReturnAssetBenchmark
{
    private ApplicationDbContext _context;
    private ReturnAsset.Handler _handler;
    private ReturnAsset.Command _command;
    private Guid _borrowingId;
    private Guid _assetId;

    [GlobalSetup]
    public void Setup()
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

    [Benchmark(Baseline = true)]
    public async Task<ReturnAsset.Result> VerticalSlice_ReturnAsset()
    {
        return await _handler.Handle(_command, CancellationToken.None);
    }
}