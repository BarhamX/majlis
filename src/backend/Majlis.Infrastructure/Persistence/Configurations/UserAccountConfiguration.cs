using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("Users", table => table.HasCheckConstraint(
            "CK_Users_Status",
            "\"Status\" IN ('active', 'suspended', 'deletion_pending', 'deleted')"));
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Status)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToUserAccountStatus(value));
        builder.Property(user => user.AuthenticationNotBefore)
            .HasColumnType("timestamp with time zone");
        builder.Property(user => user.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(user => user.LastLoginAt).HasColumnType("timestamp with time zone");
        builder.Property(user => user.DeletedAt).HasColumnType("timestamp with time zone");

        builder.HasMany(user => user.Identities)
            .WithOne()
            .HasForeignKey(identity => identity.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(user => user.Profile)
            .WithOne()
            .HasForeignKey<UserProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(user => user.Preferences)
            .WithOne()
            .HasForeignKey<UserPreferences>(preferences => preferences.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(user => user.Consents)
            .WithOne()
            .HasForeignKey(consent => consent.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(user => user.DeletionRequests)
            .WithOne()
            .HasForeignKey(request => request.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Identities).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.Consents).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.RoleAssignments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.DeletionRequests).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
