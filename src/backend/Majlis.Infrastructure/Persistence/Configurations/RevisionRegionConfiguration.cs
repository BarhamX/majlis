using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class RevisionRegionConfiguration : IEntityTypeConfiguration<RevisionRegion>
{
    public void Configure(EntityTypeBuilder<RevisionRegion> builder)
    {
        builder.ToTable("RevisionRegions");
        builder.HasKey(region => new { region.RevisionId, region.RegionCode });
        builder.Property(region => region.RegionCode).HasColumnType("text");
        builder.HasOne<DailyMajlisRevision>()
            .WithMany(revision => revision.Regions)
            .HasForeignKey(region => region.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
