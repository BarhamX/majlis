using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("UserConsents", table => table.HasCheckConstraint(
            "CK_UserConsents_Type",
            "\"Type\" IN ('terms', 'privacy', 'analytics')"));
        builder.HasKey(consent => consent.Id);
        builder.Property(consent => consent.Id).ValueGeneratedNever();
        builder.Property(consent => consent.Type)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToConsentType(value));
        builder.Property(consent => consent.Version).HasColumnType("text").IsRequired();
        builder.Property(consent => consent.RecordedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(consent => new { consent.UserId, consent.Type, consent.Version }).IsUnique();
    }
}
