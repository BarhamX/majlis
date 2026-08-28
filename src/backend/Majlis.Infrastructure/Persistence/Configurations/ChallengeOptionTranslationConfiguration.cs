using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class ChallengeOptionTranslationConfiguration : IEntityTypeConfiguration<ChallengeOptionTranslation>
{
    public void Configure(EntityTypeBuilder<ChallengeOptionTranslation> builder)
    {
        builder.ToTable("ChallengeOptionTranslations");
        builder.HasKey(translation => new { translation.OptionId, translation.Locale });
        builder.Property(translation => translation.Locale).HasColumnType("text");
        builder.Property(translation => translation.Text).HasColumnType("text").IsRequired();
        builder.HasOne<ChallengeOption>()
            .WithMany(option => option.Translations)
            .HasForeignKey(translation => translation.OptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
