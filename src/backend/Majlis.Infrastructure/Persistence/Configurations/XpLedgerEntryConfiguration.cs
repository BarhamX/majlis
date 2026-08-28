using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class XpLedgerEntryConfiguration : IEntityTypeConfiguration<XpLedgerEntry>
{
    public void Configure(EntityTypeBuilder<XpLedgerEntry> builder)
    {
        builder.ToTable("XpLedger", table => table.HasCheckConstraint(
            "CK_XpLedger_ExactAmount",
            "\"Amount\" IN (10, 15)"));
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).ValueGeneratedNever();
        builder.Property(entry => entry.UserId).IsRequired();
        builder.Property(entry => entry.AttemptId).IsRequired();
        builder.Property(entry => entry.Amount).IsRequired();
        builder.Property(entry => entry.OccurredAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(entry => entry.AttemptId)
            .IsUnique()
            .HasDatabaseName("UX_XpLedger_AttemptId");
        builder.HasIndex(entry => new { entry.OccurredAt, entry.Amount })
            .HasDatabaseName("IX_XpLedger_OccurredAt_Amount");
        builder.HasIndex(entry => new { entry.UserId, entry.OccurredAt })
            .HasDatabaseName("IX_XpLedger_UserId_OccurredAt");

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(entry => entry.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_XpLedger_Users_UserId");
        builder.HasOne<UserAttempt>()
            .WithOne()
            .HasForeignKey<XpLedgerEntry>(entry => entry.AttemptId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_XpLedger_UserAttempts_AttemptId");

        foreach (var property in builder.Metadata.GetProperties())
        {
            property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        }
    }
}
