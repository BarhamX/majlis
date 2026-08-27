using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class DailyMajlisTranslationConfiguration : IEntityTypeConfiguration<DailyMajlisTranslation>
{
    public void Configure(EntityTypeBuilder<DailyMajlisTranslation> builder)
    {
        builder.ToTable("DailyMajlisTranslations");
        builder.HasKey(translation => new { translation.RevisionId, translation.Locale });
        builder.Property(translation => translation.Locale).HasColumnType("text");
        builder.Property(translation => translation.Title).HasColumnType("text").IsRequired();
        builder.Property(translation => translation.QuestionText).HasColumnType("text").IsRequired();
        builder.Property(translation => translation.Explanation).HasColumnType("text").IsRequired();
        builder.Property(translation => translation.DiscussionPrompt).HasColumnType("text").IsRequired();
        builder.Property(translation => translation.CardTitle).HasColumnType("text");
        builder.Property(translation => translation.CardText).HasColumnType("text").IsRequired();
        builder.Property(translation => translation.CardMeaning).HasColumnType("text");
        builder.Property(translation => translation.CardContext).HasColumnType("text");
        builder.Property(translation => translation.Transliteration).HasColumnType("text");
        builder.Property(translation => translation.PublicAttribution).HasColumnType("text");
        builder.Property(translation => translation.CorrectionNote).HasColumnType("text");
        builder.HasOne<DailyMajlisRevision>()
            .WithMany(revision => revision.Translations)
            .HasForeignKey(translation => translation.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
