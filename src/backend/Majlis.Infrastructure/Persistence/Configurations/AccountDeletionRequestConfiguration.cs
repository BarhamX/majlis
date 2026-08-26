using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class AccountDeletionRequestConfiguration : IEntityTypeConfiguration<AccountDeletionRequest>
{
    public void Configure(EntityTypeBuilder<AccountDeletionRequest> builder)
    {
        builder.ToTable("AccountDeletionRequests", table => table.HasCheckConstraint(
            "CK_AccountDeletionRequests_Status",
            "\"Status\" IN ('requested', 'identity_deleted', 'active_data_purged', 'backup_expiry_pending', 'completed', 'legal_hold')"));
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.Status)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToAccountDeletionStatus(value));
        builder.Property(request => request.RequestedAt).HasColumnType("timestamp with time zone");
        builder.Property(request => request.PurgeDueAt).HasColumnType("timestamp with time zone");
        builder.Property(request => request.BackupExpiryDueAt).HasColumnType("timestamp with time zone");
        builder.Property(request => request.CompletedAt).HasColumnType("timestamp with time zone");
        builder.Property(request => request.LegalHoldReason).HasColumnType("text");
        builder.HasIndex(request => request.UserId)
            .IsUnique()
            .HasFilter("\"Status\" <> 'completed'");
    }
}
