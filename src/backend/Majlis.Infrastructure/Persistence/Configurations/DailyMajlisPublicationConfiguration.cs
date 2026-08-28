using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class DailyMajlisPublicationConfiguration :
    IEntityTypeConfiguration<DailyMajlisPublication>
{
    public void Configure(EntityTypeBuilder<DailyMajlisPublication> builder)
    {
        builder.ToTable("DailyMajlisPublications");
        builder.HasKey(publication => publication.DailyMajlisId);
        builder.Property(publication => publication.DailyMajlisId).ValueGeneratedNever();
        builder.Property(publication => publication.PublishDate).HasColumnType("date");
        builder.Property(publication => publication.PublishedAt)
            .HasColumnType("timestamp with time zone");
        builder.HasIndex(publication => publication.PublishDate).IsUnique();

        foreach (var property in builder.Metadata.GetProperties())
        {
            property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        }
    }
}
