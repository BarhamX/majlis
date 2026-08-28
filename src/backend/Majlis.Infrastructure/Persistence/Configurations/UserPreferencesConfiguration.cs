using Majlis.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Majlis.Infrastructure.Persistence.Configurations;

internal sealed class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("UserPreferences");
        builder.HasKey(preferences => preferences.UserId);
        builder.Property(preferences => preferences.ReminderLocalTime).HasColumnType("time");
        builder.Property(preferences => preferences.ReminderTimeZoneId).HasColumnType("text");
        builder.Property(preferences => preferences.UpdatedAt).HasColumnType("timestamp with time zone");
    }
}
