using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using LabResource.Application.DTOs.Users;
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
public class UpdateUserBenchmark
{
    private UserService _cleanArchitectureService = null!;
    private UpdateUserRequest _storedRequest = null!;
    private Guid _storedUserId;

    [GlobalSetup]
    public void Setup()
    {
        var userId = Guid.NewGuid();

        _storedRequest = new UpdateUserRequest
        {
            FullName = "Johnathon Doe",
            MatriculationNumber = "STU-98765432"
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Users.Add(new User
        {
            Id = userId,
            FullName = "John Doe",
            Email = "john.doe@stud.ubbcluj.ro",
            IsActive = true,
            PasswordHash = Guid.NewGuid().ToString()
        });
        context.SaveChanges();

        IUserRepository userRepo = new UserRepository(context);

        _cleanArchitectureService = new UserService(userRepo);

        _storedUserId = userId;
    }

    [Benchmark]
    public async Task<bool> CleanArchitecture_UpdateUser()
    {
        return await _cleanArchitectureService.UpdateUserAsync(_storedUserId, _storedRequest);
    }
}