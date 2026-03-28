using LabResource.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabResource.Infrastructure.Persistence.Configurations;

public class LabAssetConfiguration : IEntityTypeConfiguration<LabAsset>
{
    public void Configure(EntityTypeBuilder<LabAsset> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SerialNumber)
            .HasMaxLength(50);

        builder.HasIndex(x => x.SerialNumber)
            .IsUnique();
    }
}