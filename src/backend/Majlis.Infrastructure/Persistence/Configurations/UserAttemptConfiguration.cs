using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserAttemptConfiguration : IEntityTypeConfiguration<UserAttempt>
{
    public void Configure(EntityTypeBuilder<UserAttempt> builder)
    {
        builder.ToTable("UserAttempts", table => table.HasCheckConstraint(
            "CK_UserAttempts_ExactXpAndSnapshots",
            "\"CompletionXp\" = 10 AND \"CorrectnessXp\" IN (0, 5) " +
            "AND ((\"IsCorrect\" AND \"CorrectnessXp\" = 5) " +
            "OR (NOT \"IsCorrect\" AND \"CorrectnessXp\" = 0)) " +
            "AND \"LifetimeXpAfter\" >= 0 AND \"CurrentStreakAfter\" >= 0 " +
            "AND \"LongestStreakAfter\" >= \"CurrentStreakAfter\""));
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Id).ValueGeneratedNever();
        builder.Property(attempt => attempt.UserId).IsRequired();
        builder.Property(attempt => attempt.DailyMajlisId).IsRequired();
        builder.Property(attempt => attempt.ChallengeId).IsRequired();
        builder.Property(attempt => attempt.ContentRevisionId).IsRequired();
        builder.Property(attempt => attempt.SelectedOptionId).IsRequired();
        builder.Property(attempt => attempt.IsCorrect).IsRequired();
        builder.Property(attempt => attempt.CompletionXp).IsRequired();
        builder.Property(attempt => attempt.CorrectnessXp).IsRequired();
        builder.Property(attempt => attempt.ResultLocale).HasColumnType("text").IsRequired();
        builder.Property(attempt => attempt.LifetimeXpAfter).IsRequired();
        builder.Property(attempt => attempt.CurrentStreakAfter).IsRequired();
        builder.Property(attempt => attempt.LongestStreakAfter).IsRequired();
        builder.Property(attempt => attempt.AttemptedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(attempt => new { attempt.UserId, attempt.DailyMajlisId })
            .IsUnique()
            .HasDatabaseName("UX_UserAttempts_UserId_DailyMajlisId");
        builder.HasIndex(attempt => new { attempt.UserId, attempt.AttemptedAt, attempt.Id })
            .IsDescending(false, true, true)
            .HasDatabaseName("IX_UserAttempts_UserId_AttemptedAt_Id");

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(attempt => attempt.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_UserAttempts_Users_UserId");
        builder.HasOne<DailyMajlisEntity>()
            .WithMany()
            .HasForeignKey(attempt => attempt.DailyMajlisId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UserAttempts_DailyMajlis_DailyMajlisId");
        builder.HasOne<Challenge>()
            .WithMany()
            .HasForeignKey(attempt => new { attempt.ChallengeId, attempt.ContentRevisionId })
            .HasPrincipalKey(challenge => new { challenge.Id, challenge.RevisionId })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UserAttempts_Challenges_ChallengeId_ContentRevisionId");
        builder.HasOne<ChallengeOption>()
            .WithMany()
            .HasForeignKey(attempt => new { attempt.SelectedOptionId, attempt.ChallengeId })
            .HasPrincipalKey("Id", "ChallengeId")
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UserAttempts_ChallengeOptions_SelectedOptionId_ChallengeId");

        foreach (var property in builder.Metadata.GetProperties())
        {
            property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        }
    }
}
