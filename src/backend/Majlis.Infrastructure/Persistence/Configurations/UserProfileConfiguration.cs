using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("Profiles", table =>
        {
            table.HasCheckConstraint(
                "CK_Profiles_AgeBand",
                "\"AgeBand\" IN ('13_17', '18_plus')");
            table.HasCheckConstraint(
                "CK_Profiles_LeaderboardVisibility",
                "\"LeaderboardVisibility\" IN ('private', 'global_weekly')");
            table.HasCheckConstraint(
                "CK_Profiles_DisplayNameStorageLength",
                "char_length(btrim(\"DisplayName\")) BETWEEN 1 AND 120");
            table.HasCheckConstraint(
                "CK_Profiles_CountryCode",
                "\"CountryCode\" IS NULL OR \"CountryCode\" ~ '^[A-Z]{2}$'");
        });
        builder.HasKey(profile => profile.UserId);
        builder.Property(profile => profile.DisplayName).HasColumnType("text").IsRequired();
        builder.Property(profile => profile.DisplayNameNormalized).HasColumnType("text").IsRequired();
        builder.Property(profile => profile.AgeBand)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToAgeBand(value));
        builder.Property(profile => profile.AgeBandAttestedAt)
            .HasColumnType("timestamp with time zone");
        builder.Property(profile => profile.CountryCode).HasColumnType("character(2)");
        builder.Property(profile => profile.RegionCode).HasColumnType("text");
        builder.Property(profile => profile.DialectCode).HasColumnType("text");
        builder.Property(profile => profile.Locale).HasColumnType("text").IsRequired();
        builder.Property(profile => profile.LeaderboardVisibility)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToLeaderboardVisibility(value));
        builder.Property(profile => profile.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(profile => profile.UpdatedAt).HasColumnType("timestamp with time zone");
    }
}
