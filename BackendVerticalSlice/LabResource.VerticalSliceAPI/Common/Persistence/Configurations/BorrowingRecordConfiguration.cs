using LabResource.VerticalApi.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabResource.VerticalApi.Common.Persistence.Configurations;

public class BorrowingRecordConfiguration : IEntityTypeConfiguration<BorrowingRecord>
{
    public void Configure(EntityTypeBuilder<BorrowingRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Remarks)
            .HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany(u => u.BorrowingRecords)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LabAsset)
            .WithMany(a => a.BorrowingRecords)
            .HasForeignKey(x => x.LabAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}