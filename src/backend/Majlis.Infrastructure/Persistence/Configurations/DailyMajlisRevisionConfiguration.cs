using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class DailyMajlisRevisionConfiguration : IEntityTypeConfiguration<DailyMajlisRevision>
{
    public void Configure(EntityTypeBuilder<DailyMajlisRevision> builder)
    {
        builder.ToTable("DailyMajlisRevisions", table => table.HasCheckConstraint(
            "CK_DailyMajlisRevisions_SourceNotes",
            "length(btrim(\"SourceNotes\")) > 0"));
        builder.HasKey(revision => revision.Id);
        builder.Property(revision => revision.Id).ValueGeneratedNever();
        builder.Property(revision => revision.DailyMajlisId).IsRequired();
        builder.Property(revision => revision.RevisionNumber).IsRequired();
        builder.Property(revision => revision.TopicCode).HasColumnType("text").IsRequired();
        builder.Property(revision => revision.Difficulty)
            .HasColumnType("text")
            .HasConversion(
                value => EnumStorage.ToStorage(value),
                value => EnumStorage.ToChallengeDifficulty(value));
        builder.Property(revision => revision.CardType)
            .HasColumnType("text")
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => Enum.Parse<CardType>(value, ignoreCase: true));
        builder.Property(revision => revision.SourceNotes).HasColumnType("text").IsRequired();
        builder.Property(revision => revision.CreatedByUserId);
        builder.Property(revision => revision.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(revision => revision.SubmittedAt).HasColumnType("timestamp with time zone");
        builder.Property(revision => revision.SupersedesRevisionId);

        builder.HasIndex(revision => new { revision.DailyMajlisId, revision.RevisionNumber }).IsUnique();
        builder.HasOne<DailyMajlisEntity>()
            .WithMany()
            .HasForeignKey(revision => revision.DailyMajlisId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(revision => revision.SupersedesRevisionId);
        builder.HasOne<DailyMajlisRevision>()
            .WithMany()
            .HasForeignKey(revision => revision.SupersedesRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(revision => revision.Challenge)
            .WithOne()
            .HasForeignKey<Challenge>(challenge => challenge.RevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(revision => revision.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(revision => revision.Regions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(revision => revision.Dialects)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
