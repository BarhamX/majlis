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
        builder.Property(option => option.Text).HasColumnType("text").IsRequired();
        builder.Property(option => option.IsCorrect).IsRequired();
        builder.Property(option => option.SortOrder).IsRequired();
        builder.Property<Guid>("ChallengeId");
    }
}
