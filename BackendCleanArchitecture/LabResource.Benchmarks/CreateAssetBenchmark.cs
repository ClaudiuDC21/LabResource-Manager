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
public class CreateAssetBenchmark
{
    private LabAssetService _cleanArchitectureService = null!;
    private CreateLabAssetRequest _request = null!;

    [IterationSetup]
    public void IterationSetup()
    {
        var teacherId = Guid.NewGuid();

        _request = new CreateLabAssetRequest
        {
            Name = "Electron Microscope",
            SerialNumber = "EM-5000X",
            Location = "Biology Lab B201",
            AssignedTeacherId = teacherId
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Users.Add(new User
        {
            Id = teacherId,
            Role = UserRole.Teacher,
            IsActive = true,
            FullName = "Dr. Sarah Jenkins",
            Email = "sarah.jenkins@ubbcluj.ro",
            PasswordHash = Guid.NewGuid().ToString()
        });

        context.SaveChanges();

        ILabAssetRepository assetRepo = new LabAssetRepository(context);
        IUserRepository userRepo = new UserRepository(context);

        _cleanArchitectureService = new LabAssetService(assetRepo, userRepo);
    }

    [Benchmark]
    public async Task<LabAssetResponse> CleanArchitecture_CreateAsset()
    {
        return await _cleanArchitectureService.CreateAssetAsync(_request);
    }
}