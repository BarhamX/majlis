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
        builder.Property(challenge => challenge.RevisionId);
        builder.Property(challenge => challenge.RevisionId).IsRequired();
        builder.Property(challenge => challenge.Type)
            .HasColumnType("text")
            .HasConversion(
                type => EnumStorage.ToStorage(type),
                value => EnumStorage.ToChallengeType(value));
        builder.HasAlternateKey(challenge => new { challenge.Id, challenge.RevisionId })
            .HasName("AK_Challenges_Id_RevisionId");
        builder.HasIndex(challenge => challenge.RevisionId).IsUnique();

        builder.HasMany(challenge => challenge.Options)
            .WithOne()
            .HasForeignKey("ChallengeId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(challenge => challenge.Options)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
