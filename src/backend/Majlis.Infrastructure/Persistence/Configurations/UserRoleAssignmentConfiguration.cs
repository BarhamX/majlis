using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable("UserRoleAssignments", table => table.HasCheckConstraint(
            "CK_UserRoleAssignments_Role",
            "\"Role\" IN ('moderator', 'content_editor', 'content_reviewer', 'publisher', 'operations_admin')"));
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Id).ValueGeneratedNever();
        builder.Property(assignment => assignment.Role)
            .HasColumnType("text")
            .HasConversion(
                value => IdentityStorage.ToStorage(value),
                value => IdentityStorage.ToUserRole(value));
        builder.Property(assignment => assignment.AssignedAt)
            .HasColumnType("timestamp with time zone");
        builder.Property(assignment => assignment.RevokedAt)
            .HasColumnType("timestamp with time zone");
        builder.HasIndex(assignment => new { assignment.UserId, assignment.Role })
            .IsUnique()
            .HasFilter("\"RevokedAt\" IS NULL");
        builder.HasOne<UserAccount>()
            .WithMany(user => user.RoleAssignments)
            .HasForeignKey(assignment => assignment.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(assignment => assignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
