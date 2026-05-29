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

    [IterationSetup]
    public void IterationSetup()
    {
        var userId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        var request = new BorrowAssetRequest
        {
            UserId = userId,
            LabAssetId = assetId,
            RequestedStartDate = DateTime.UtcNow.AddDays(1),
            RequestedEndDate = DateTime.UtcNow.AddDays(3)
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Users.Add(new User
        {
            Id = userId,
            IsActive = true,
            FullName = "Emily Chen",
            Email = "emily.chen@stud.ubbcluj.ro",
            PasswordHash = Guid.NewGuid().ToString()
        });

        context.LabAssets.Add(new LabAsset
        {
            Id = assetId,
            IsActive = true,
            Status = AssetStatus.Available,
            AssignedTeacherId = Guid.NewGuid(),
            Name = "Mass Spectrometer"
        });

        context.SaveChanges();

        IUserRepository userRepo = new UserRepository(context);
        ILabAssetRepository assetRepo = new LabAssetRepository(context);
        IBorrowingRecordRepository borrowingRepo = new BorrowingRecordRepository(context);

        _cleanArchitectureService = new BorrowingService(userRepo, assetRepo, borrowingRepo);
        _storedRequest = request;
    }

    private BorrowAssetRequest _storedRequest = null!;

    [Benchmark]
    public async Task<BorrowingResponse> CleanArchitecture_RequestAsset()
    {
        return await _cleanArchitectureService.RequestAssetAsync(_storedRequest);
    }
}