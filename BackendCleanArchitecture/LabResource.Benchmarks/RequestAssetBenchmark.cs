using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.Borrowings;
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
public class RequestAssetBenchmark
{
    private BorrowingService _cleanArchitectureService = null!;
    private BorrowAssetRequest _request = null!;
    private Guid _userId;
    private Guid _assetId;
    private ApplicationDbContext _context = null!;

    [IterationSetup]
    public void IterationSetup()
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

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Users.Add(new User
        {
            Id = _userId,
            IsActive = true,
            FullName = "Emily Chen",
            Email = "emily.chen@stud.ubbcluj.ro",
            PasswordHash = "hash"
        });

        _context.LabAssets.Add(new LabAsset
        {
            Id = _assetId,
            IsActive = true,
            Status = AssetStatus.Available,
            AssignedTeacherId = Guid.NewGuid(),
            Name = "Mass Spectrometer"
        });

        _context.SaveChanges();

        IUserRepository userRepo = new UserRepository(_context);
        ILabAssetRepository assetRepo = new LabAssetRepository(_context);
        IBorrowingRecordRepository borrowingRepo = new BorrowingRecordRepository(_context);

        _cleanArchitectureService = new BorrowingService(userRepo, assetRepo, borrowingRepo);
    }

    [Benchmark]
    public async Task<BorrowingResponse> CleanArchitecture_RequestAsset()
    {
        return await _cleanArchitectureService.RequestAssetAsync(_request);
    }
}