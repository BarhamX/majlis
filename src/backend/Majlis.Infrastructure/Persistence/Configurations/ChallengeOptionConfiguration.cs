using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class ChallengeOptionConfiguration : IEntityTypeConfiguration<ChallengeOption>
{
    public void Configure(EntityTypeBuilder<ChallengeOption> builder)
    {
        builder.ToTable("ChallengeOptions");
        builder.HasKey(option => option.Id);

        builder.Property(option => option.Id).ValueGeneratedNever();
        builder.Property(option => option.OptionKey).HasColumnType("text").IsRequired();
        builder.HasIndex("ChallengeId", nameof(ChallengeOption.OptionKey)).IsUnique();
        builder.HasIndex("ChallengeId", nameof(ChallengeOption.SortOrder)).IsUnique();
        builder.Property(option => option.IsCorrect).IsRequired();
        builder.Property(option => option.SortOrder).IsRequired();
        builder.Property<Guid>("ChallengeId");
        builder.HasAlternateKey("Id", "ChallengeId")
            .HasName("AK_ChallengeOptions_Id_ChallengeId");
        builder.HasMany(option => option.Translations)
            .WithOne()
            .HasForeignKey(translation => translation.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(option => option.Translations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
