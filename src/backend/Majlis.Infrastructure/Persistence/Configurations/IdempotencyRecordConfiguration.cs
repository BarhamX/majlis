using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(record => new
        {
            record.UserId,
            record.Scope,
            record.IdempotencyKey,
        })
            .HasName("PK_IdempotencyRecords");
        builder.Property(record => record.UserId).IsRequired();
        builder.Property(record => record.Scope).HasColumnType("text").IsRequired();
        builder.Property(record => record.IdempotencyKey).ValueGeneratedNever();
        builder.Property(record => record.RequestHash).HasColumnType("text").IsRequired();
        builder.Property(record => record.ResourceId);
        builder.Property(record => record.ResponseStatus).IsRequired();
        builder.Property(record => record.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(record => record.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(record => record.ExpiresAt)
            .HasDatabaseName("IX_IdempotencyRecords_ExpiresAt");
        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(record => record.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_IdempotencyRecords_Users_UserId");

        foreach (var property in builder.Metadata.GetProperties())
        {
            property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        }
    }
}
