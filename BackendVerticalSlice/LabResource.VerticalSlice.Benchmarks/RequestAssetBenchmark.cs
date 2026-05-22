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

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "VerticalSlice Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class RequestAssetBenchmark
{
    private ApplicationDbContext _context;
    private RequestAsset.Handler _handler;
    private RequestAsset.Command _command;
    private Guid _userId;
    private Guid _assetId;

    [IterationSetup] 
    public void IterationSetup()
    {
        _userId = Guid.NewGuid();
        _assetId = Guid.NewGuid();

        _command = new RequestAsset.Command(
            _userId,
            _assetId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3));

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Users.Add(new User { Id = _userId, IsActive = true, FullName = "Emily Chen", Email = "emily.chen@stud.ubbcluj.ro", PasswordHash = "hash" });
        _context.LabAssets.Add(new LabAsset { Id = _assetId, IsActive = true, Status = AssetStatus.Available, Name = "Mass Spectrometer" });
        _context.SaveChanges();

        _handler = new RequestAsset.Handler(_context);
    }

    [Benchmark]
    public async Task<RequestAsset.Result> VerticalSlice_RequestAsset()
    {
        return await _handler.Handle(_command, CancellationToken.None);
    }
}