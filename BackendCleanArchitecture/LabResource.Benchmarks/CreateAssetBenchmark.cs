using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.LabAssets;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;

namespace LabResource.Benchmarks;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "CleanArchitecture Thesis")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class CreateAssetBenchmark
{
    private LabAssetService _cleanArchitectureService = null!;
    private CreateLabAssetRequest _request = null!;
    private Guid _teacherId;

    [GlobalSetup]
    public void Setup()
    {
        _teacherId = Guid.NewGuid();

        _request = new CreateLabAssetRequest
        {
            Name = "Electron Microscope",
            SerialNumber = "EM-5000X",
            Location = "Biology Lab B201",
            AssignedTeacherId = _teacherId
        };

        var mockAssetRepo = new Mock<ILabAssetRepository>();
        mockAssetRepo
            .Setup(repo => repo.GetBySerialNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((LabAsset?)null);

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo
            .Setup(repo => repo.GetByIdAsync(_teacherId))
            .ReturnsAsync(new User
            {
                Id = _teacherId,
                Role = UserRole.Teacher,
                IsActive = true,
                FullName = "Dr. Sarah Jenkins",
                Email = "sarah.jenkins@ubbcluj.ro"
            });

        _cleanArchitectureService = new LabAssetService(mockAssetRepo.Object, mockUserRepo.Object);
    }

    [Benchmark]
    public async Task<LabAssetResponse> CleanArchitecture_CreateAsset()
    {
        return await _cleanArchitectureService.CreateAssetAsync(_request);
    }
}