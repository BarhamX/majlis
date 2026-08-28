namespace Majlis.Domain.Identity;

public sealed class UserPreferences
{
    private UserPreferences()
    {
    }

    internal UserPreferences(Guid userId, DateTimeOffset now)
    {
        UserId = userId;
        UpdatedAt = now;
    }

    public Guid UserId { get; private set; }

    public bool ReminderEnabled { get; private set; }

    public TimeOnly? ReminderLocalTime { get; private set; }

    public string? ReminderTimeZoneId { get; private set; }

    public bool AnalyticsConsent { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
}
