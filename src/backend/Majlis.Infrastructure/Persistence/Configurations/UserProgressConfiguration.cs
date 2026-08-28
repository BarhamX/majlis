using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("UserProgress", table => table.HasCheckConstraint(
            "CK_UserProgress_NonNegative",
            "\"LifetimeXp\" >= 0 AND \"CurrentStreak\" >= 0 " +
            "AND \"LongestStreak\" >= \"CurrentStreak\""));
        builder.HasKey(progress => progress.UserId);
        builder.Property(progress => progress.UserId).ValueGeneratedNever();
        builder.Property(progress => progress.LifetimeXp).HasDefaultValue(0L).IsRequired();
        builder.Property(progress => progress.CurrentStreak).HasDefaultValue(0).IsRequired();
        builder.Property(progress => progress.LongestStreak).HasDefaultValue(0).IsRequired();
        builder.Property(progress => progress.LastCompletedPublishDate).HasColumnType("date");
        builder.Property(progress => progress.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<UserAccount>()
            .WithOne()
            .HasForeignKey<UserProgress>(progress => progress.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_UserProgress_Users_UserId");
    }
}
