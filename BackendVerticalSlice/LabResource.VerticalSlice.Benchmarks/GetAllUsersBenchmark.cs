using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Users;
using Microsoft.EntityFrameworkCore;

[MemoryDiagnoser]
public class GetAllUsersBenchmark
{
    private ApplicationDbContext _context;
    private GetAllActiveUsers.Handler _handler;
    private GetAllActiveUsers.Query _query;

    [GlobalSetup]
    public void Setup()
    {
        _query = new GetAllActiveUsers.Query();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "Alice Johnson", Email = "alice.johnson@stud.ubbcluj.ro", Role = UserRole.Student, IsActive = true, PasswordHash = "hash" },
            new User { Id = Guid.NewGuid(), FullName = "Dr. Robert Smith", Email = "robert.smith@ubbcluj.ro", Role = UserRole.Teacher, IsActive = true, PasswordHash = "hash" },
            new User { Id = Guid.NewGuid(), FullName = "Charlie Davis", Email = "charlie.davis@stud.ubbcluj.ro", Role = UserRole.Student, IsActive = true, PasswordHash = "hash" }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();

        _handler = new GetAllActiveUsers.Handler(_context);
    }

    [Benchmark(Baseline = true)]
    public async Task<IEnumerable<GetAllActiveUsers.Result>> VerticalSlice_GetAllUsers()
    {
        return await _handler.Handle(_query, CancellationToken.None);
    }
}