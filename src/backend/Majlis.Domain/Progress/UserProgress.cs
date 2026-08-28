namespace Majlis.Domain.Progress;

public sealed class UserProgress
{
    private UserProgress()
    {
    }

    public UserProgress(Guid userId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A user id is required.", nameof(userId));
        }

        UserId = userId;
        UpdatedAt = now;
    }

    public Guid UserId { get; private set; }

    public long LifetimeXp { get; private set; }

    public int CurrentStreak { get; private set; }

    public int LongestStreak { get; private set; }

    public DateOnly? LastCompletedPublishDate { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void ApplyAttempt(
        AttemptScore score,
        DateOnly completedPublishDate,
        IReadOnlyCollection<DateOnly> publishedDates,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(score);
        ArgumentNullException.ThrowIfNull(publishedDates);

        if (!publishedDates.Contains(completedPublishDate))
        {
            throw new ArgumentException(
                "The completed date must be an eligible published content day.",
                nameof(completedPublishDate));
        }

        if (LastCompletedPublishDate == completedPublishDate)
        {
            return;
        }

        if (LastCompletedPublishDate > completedPublishDate)
        {
            throw new InvalidOperationException("Progress cannot be applied before the last completed publish date.");
        }

        LifetimeXp = checked(LifetimeXp + score.TotalXp);

        if (LastCompletedPublishDate is null)
        {
            CurrentStreak = 1;
        }
        else
        {
            var skippedPublishedDay = publishedDates.Any(date =>
                date > LastCompletedPublishDate.Value && date < completedPublishDate);
            CurrentStreak = skippedPublishedDay ? 1 : checked(CurrentStreak + 1);
        }

        LongestStreak = Math.Max(LongestStreak, CurrentStreak);
        LastCompletedPublishDate = completedPublishDate;
        UpdatedAt = now;
    }
}
