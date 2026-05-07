using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.Users;
using Microsoft.EntityFrameworkCore;

[MemoryDiagnoser]
public class UpdateUserBenchmark
{
    private ApplicationDbContext _context;
    private UpdateUser.Handler _handler;
    private UpdateUser.Command _command;
    private Guid _userId;

    [GlobalSetup]
    public void Setup()
    {
        _userId = Guid.NewGuid();

        _command = new UpdateUser.Command(_userId, "Johnathon Doe", "STU-98765432");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Users.Add(new User
        {
            Id = _userId,
            FullName = "John Doe",
            IsActive = true,
            Email = "john.doe@stud.ubbcluj.ro",
            PasswordHash = "hash"
        });
        _context.SaveChanges();

        _handler = new UpdateUser.Handler(_context);
    }

    [Benchmark(Baseline = true)]
    public async Task VerticalSlice_UpdateUser()
    {
        await _handler.Handle(_command, CancellationToken.None);
    }
}