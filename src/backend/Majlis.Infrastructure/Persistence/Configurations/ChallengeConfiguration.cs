using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> builder)
    {
        builder.ToTable("Challenges");
        builder.HasKey(challenge => challenge.Id);

        builder.Property(challenge => challenge.Id).ValueGeneratedNever();
        builder.Property(challenge => challenge.QuestionText).HasColumnType("text").IsRequired();
        builder.Property(challenge => challenge.Type)
            .HasColumnType("text")
            .HasConversion(
                type => EnumStorage.ToStorage(type),
                value => EnumStorage.ToChallengeType(value));
        builder.Property(challenge => challenge.Difficulty)
            .HasColumnType("text")
            .HasConversion(
                difficulty => EnumStorage.ToStorage(difficulty),
                value => EnumStorage.ToChallengeDifficulty(value));
        builder.Property(challenge => challenge.Region).HasColumnType("text");
        builder.Property(challenge => challenge.Topic).HasColumnType("text").IsRequired();
        builder.Property(challenge => challenge.Explanation).HasColumnType("text").IsRequired();
        builder.Property(challenge => challenge.SourceNotes).HasColumnType("text");
        builder.Property(challenge => challenge.ReviewStatus)
            .HasColumnType("text")
            .HasConversion(
                status => EnumStorage.ToStorage(status),
                value => EnumStorage.ToContentReviewStatus(value));
        builder.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");

        builder.HasMany(challenge => challenge.Options)
            .WithOne()
            .HasForeignKey("ChallengeId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(challenge => challenge.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
