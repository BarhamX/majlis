using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class DailyMajlisConfiguration : IEntityTypeConfiguration<DailyMajlisEntity>
{
    public void Configure(EntityTypeBuilder<DailyMajlisEntity> builder)
    {
        builder.ToTable("DailyMajlis");
        builder.HasKey(dailyMajlis => dailyMajlis.Id);

        builder.Property(dailyMajlis => dailyMajlis.Id).ValueGeneratedNever();
        builder.Property(dailyMajlis => dailyMajlis.PublishDate).HasColumnType("date");
        builder.Property(dailyMajlis => dailyMajlis.Status)
            .HasColumnType("text")
            .HasConversion(
                status => EnumStorage.ToStorage(status),
                value => EnumStorage.ToDailyMajlisStatus(value));
        builder.Property(dailyMajlis => dailyMajlis.ScheduledRevisionId);
        builder.Property(dailyMajlis => dailyMajlis.PublishedRevisionId);

        builder.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
        builder.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone");

        builder.HasOne(dailyMajlis => dailyMajlis.ScheduledRevision)
            .WithMany()
            .HasForeignKey(dailyMajlis => dailyMajlis.ScheduledRevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dailyMajlis => dailyMajlis.PublishedRevision)
            .WithMany()
            .HasForeignKey(dailyMajlis => dailyMajlis.PublishedRevisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(dailyMajlis => dailyMajlis.PublishDate)
            .IsUnique()
            .HasFilter("\"Status\" IN ('scheduled', 'published')");
        builder.HasIndex(dailyMajlis => dailyMajlis.ScheduledRevisionId);
        builder.HasIndex(dailyMajlis => dailyMajlis.PublishedRevisionId);
    }
}
