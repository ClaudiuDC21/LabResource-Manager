using System;
using System.Collections.Generic;
using System.Linq;
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
public class GetAllUsersBenchmark
{
    private UserService _cleanArchitectureService = null!;
    private ApplicationDbContext _context = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "Alice Johnson", Email = "alice.johnson@stud.ubbcluj.ro", Role = UserRole.Student, IsActive = true, PasswordHash = "test_data_no_secret_123" },
            new User { Id = Guid.NewGuid(), FullName = "Dr. Robert Smith", Email = "robert.smith@ubbcluj.ro", Role = UserRole.Teacher, IsActive = true, PasswordHash = "test_data_no_secret_123" },
            new User { Id = Guid.NewGuid(), FullName = "Charlie Davis", Email = "charlie.davis@stud.ubbcluj.ro", Role = UserRole.Student, IsActive = true, PasswordHash = "test_data_no_secret_123" }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();

        IUserRepository userRepo = new UserRepository(_context);

        _cleanArchitectureService = new UserService(userRepo);
    }

    [Benchmark]
    public async Task<List<UserResponse>> CleanArchitecture_GetAllUsers()
    {
        var users = await _cleanArchitectureService.GetAllActiveUsersAsync();
        return users.ToList();
    }
}