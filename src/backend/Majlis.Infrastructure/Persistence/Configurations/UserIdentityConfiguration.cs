using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("UserIdentities", table => table.HasCheckConstraint(
            "CK_UserIdentities_Provider",
            "\"Provider\" IN ('google', 'apple', 'meta', 'snapchat', 'test')"));
        builder.HasKey(identity => identity.Id);
        builder.Property(identity => identity.Id).ValueGeneratedNever();
        builder.Property(identity => identity.Provider)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToExternalIdentityProvider(value));
        builder.Property(identity => identity.Issuer).HasColumnType("text").IsRequired();
        builder.Property(identity => identity.Subject).HasColumnType("text").IsRequired();
        builder.Property(identity => identity.RevocationHandleCiphertext).HasColumnType("bytea");
        builder.Property(identity => identity.RevocationKeyVersion).HasColumnType("text");
        builder.Property(identity => identity.LinkedAt).HasColumnType("timestamp with time zone");
        builder.Property(identity => identity.LastAuthenticatedAt).HasColumnType("timestamp with time zone");
        builder.Property(identity => identity.ProviderAuthorizationRevokedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(identity => new { identity.Issuer, identity.Subject }).IsUnique();
        builder.HasIndex(identity => new { identity.UserId, identity.Provider }).IsUnique();
    }
}
