using LabResource.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabResource.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LabAsset> LabAssets => Set<LabAsset>();
    public DbSet<BorrowingRecord> BorrowingRecords => Set<BorrowingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}