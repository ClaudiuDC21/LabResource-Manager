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
    private ReturnAssetRequest _storedRequest = null!;
    private Guid _storedBorrowingId;

    [IterationSetup]
    public void IterationSetup()
    {
        var borrowingId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new ReturnAssetRequest
        {
            Remarks = "Returned in good condition.",
            IsDefective = false
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Users.Add(new User
        {
            Id = userId,
            IsActive = true,
            FullName = "Test User",
            Email = "test.user@stud.ubbcluj.ro",
            PasswordHash = Guid.NewGuid().ToString()
        });

        context.LabAssets.Add(new LabAsset
        {
            Id = assetId,
            Status = AssetStatus.Borrowed,
            Name = "Thermal Camera",
            IsActive = true
        });

        context.BorrowingRecords.Add(new BorrowingRecord
        {
            Id = borrowingId,
            LabAssetId = assetId,
            UserId = userId,
            Status = BorrowingStatus.Active,
            Remarks = "Picked up on Monday."
        });

        context.SaveChanges();

        IUserRepository userRepo = new UserRepository(context);
        ILabAssetRepository assetRepo = new LabAssetRepository(context);
        IBorrowingRecordRepository borrowingRepo = new BorrowingRecordRepository(context);

        _cleanArchitectureService = new BorrowingService(userRepo, assetRepo, borrowingRepo);

        _storedBorrowingId = borrowingId;
        _storedRequest = request;
    }

    [Benchmark]
    public async Task<ReturnAssetResponse> CleanArchitecture_ReturnAsset()
    {
        return await _cleanArchitectureService.ReturnAssetAsync(_storedBorrowingId, _storedRequest);
    }
}