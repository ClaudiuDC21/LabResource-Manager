using LabResource.VerticalApi.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabResource.VerticalApi.Common.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<LabAsset> LabAssets => Set<LabAsset>();
    public virtual DbSet<BorrowingRecord> BorrowingRecords => Set<BorrowingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}