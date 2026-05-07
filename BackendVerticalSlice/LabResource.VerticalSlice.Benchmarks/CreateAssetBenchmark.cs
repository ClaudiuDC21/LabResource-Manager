using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using LabResource.VerticalApi.Common.Entities;
using LabResource.VerticalApi.Common.Enums;
using LabResource.VerticalApi.Common.Persistence;
using LabResource.VerticalApi.Features.LabAssets;
using Microsoft.EntityFrameworkCore;

[MemoryDiagnoser]
public class CreateAssetBenchmark
{
    private ApplicationDbContext _context;
    private CreateLabAsset.Handler _handler;
    private CreateLabAsset.Command _command;
    private Guid _teacherId;

    [GlobalSetup]
    public void Setup()
    {
        _teacherId = Guid.NewGuid();

        _command = new CreateLabAsset.Command(
            "Electron Microscope",
            "EM-5000X",
            "Biology Lab B201",
            _teacherId);

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

        _handler = new CreateLabAsset.Handler(_context);
    }

    [Benchmark(Baseline = true)]
    public async Task<CreateLabAsset.Result> VerticalSlice_CreateAsset()
    {
        return await _handler.Handle(_command, CancellationToken.None);
    }
}