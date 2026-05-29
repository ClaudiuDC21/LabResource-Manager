using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.LabAssets;
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
    private Guid _teacherId;
    private ApplicationDbContext _context = null!;

    [IterationSetup]
    public void IterationSetup()
    {
        _teacherId = Guid.NewGuid();

        _request = new CreateLabAssetRequest
        {
            Name = "Electron Microscope",
            SerialNumber = "EM-5000X",
            Location = "Biology Lab B201",
            AssignedTeacherId = _teacherId
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Users.Add(new User
        {
            Id = _teacherId,
            Role = UserRole.Teacher,
            IsActive = true,
            FullName = "Dr. Sarah Jenkins",
            Email = "sarah.jenkins@ubbcluj.ro",
            PasswordHash = "hash"
        });
        _context.SaveChanges();

        var assetRepo = new LabAssetRepository(_context);
        var userRepo = new UserRepository(_context);

        _cleanArchitectureService = new LabAssetService(assetRepo, userRepo);
    }

    [Benchmark]
    public async Task<LabAssetResponse> CleanArchitecture_CreateAsset()
    {
        return await _cleanArchitectureService.CreateAssetAsync(_request);
    }
}