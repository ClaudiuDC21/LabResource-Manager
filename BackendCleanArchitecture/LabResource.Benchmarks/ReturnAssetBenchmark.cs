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
public class ReturnAssetBenchmark
{
    private BorrowingService _cleanArchitectureService = null!;
    private ReturnAssetRequest _request = null!;
    private Guid _borrowingId;
    private Guid _assetId;
    private Guid _userId;
    private ApplicationDbContext _context = null!;

    [IterationSetup]
    public void IterationSetup()
    {
        _borrowingId = Guid.NewGuid();
        _assetId = Guid.NewGuid();
        _userId = Guid.NewGuid();

        _request = new ReturnAssetRequest
        {
            Remarks = "Returned in good condition.",
            IsDefective = false
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Users.Add(new User
        {
            Id = _userId,
            IsActive = true,
            FullName = "Test User",
            Email = "test.user@stud.ubbcluj.ro",
            PasswordHash = "test_data_no_secret_123"
        });

        _context.LabAssets.Add(new LabAsset
        {
            Id = _assetId,
            Status = AssetStatus.Borrowed,
            Name = "Thermal Camera",
            IsActive = true
        });

        _context.BorrowingRecords.Add(new BorrowingRecord
        {
            Id = _borrowingId,
            LabAssetId = _assetId,
            UserId = _userId,
            Status = BorrowingStatus.Active,
            Remarks = "Picked up on Monday."
        });

        _context.SaveChanges();

        IUserRepository userRepo = new UserRepository(_context);
        ILabAssetRepository assetRepo = new LabAssetRepository(_context);
        IBorrowingRecordRepository borrowingRepo = new BorrowingRecordRepository(_context);

        _cleanArchitectureService = new BorrowingService(userRepo, assetRepo, borrowingRepo);
    }

    [Benchmark]
    public async Task<ReturnAssetResponse> CleanArchitecture_ReturnAsset()
    {
        return await _cleanArchitectureService.ReturnAssetAsync(_borrowingId, _request);
    }
}