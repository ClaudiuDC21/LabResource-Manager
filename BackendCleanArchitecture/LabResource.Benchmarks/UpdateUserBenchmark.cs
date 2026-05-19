using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.Users;
using LabResource.Application.Interfaces.Repositories;
using LabResource.Application.Services;
using LabResource.Domain.Entities;
using LabResource.Domain.Enums;
using Moq;

[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 3, iterationCount: 30, id: "ThesisJob")]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[MemoryDiagnoser]
public class UpdateUserBenchmark
{
    private UserService _cleanArchitectureService;
    private UpdateUserRequest _request;
    private Guid _userId;

    [GlobalSetup]
    public void Setup()
    {
        _userId = Guid.NewGuid();

        _request = new UpdateUserRequest
        {
            FullName = "Johnathon Doe",
            MatriculationNumber = "STU-98765432"
        };

        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo
            .Setup(repo => repo.GetByIdAsync(_userId))
            .ReturnsAsync(new User
            {
                Id = _userId,
                FullName = "John Doe",
                Email = "john.doe@stud.ubbcluj.ro",
                IsActive = true
            });

        _cleanArchitectureService = new UserService(mockUserRepo.Object);
    }

    [Benchmark(Baseline = true)]
    public async Task<bool> CleanArchitecture_UpdateUser()
    {
        return await _cleanArchitectureService.UpdateUserAsync(_userId, _request);
    }
}