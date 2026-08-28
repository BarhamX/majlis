using Majlis.Domain.Progress;

namespace Majlis.Tests.Domain;

public sealed class UserProgressServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyAttempt_FirstCompletion_StartsStreakAtOne()
    {
        var progress = CreateProgress();

        progress.ApplyAttempt(
            AttemptScoring.Calculate(isCorrect: true),
            new DateOnly(2026, 8, 20),
            [new DateOnly(2026, 8, 20)],
            Now);

        Assert.Equal(15, progress.LifetimeXp);
        Assert.Equal(1, progress.CurrentStreak);
        Assert.Equal(1, progress.LongestStreak);
        Assert.Equal(new DateOnly(2026, 8, 20), progress.LastCompletedPublishDate);
        Assert.Equal(Now, progress.UpdatedAt);
    }

    [Fact]
    public void ApplyAttempt_ConsecutivePublishedDay_IncrementsStreak()
    {
        var progress = CreateProgress();
        Apply(progress, new DateOnly(2026, 8, 20), [new DateOnly(2026, 8, 20)]);

        Apply(
            progress,
            new DateOnly(2026, 8, 21),
            [new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 21)]);

        Assert.Equal(2, progress.CurrentStreak);
        Assert.Equal(2, progress.LongestStreak);
    }

    [Fact]
    public void ApplyAttempt_WhenPublishedDayWasSkipped_ResetsStreakToOne()
    {
        var progress = CreateProgress();
        Apply(progress, new DateOnly(2026, 8, 20), [new DateOnly(2026, 8, 20)]);

        Apply(
            progress,
            new DateOnly(2026, 8, 22),
            [
                new DateOnly(2026, 8, 20),
                new DateOnly(2026, 8, 21),
                new DateOnly(2026, 8, 22),
            ]);

        Assert.Equal(1, progress.CurrentStreak);
        Assert.Equal(1, progress.LongestStreak);
    }

    [Fact]
    public void MissingPublicationDoesNotBreakStreak()
    {
        var progress = CreateProgress();
        Apply(progress, new DateOnly(2026, 8, 20), [new DateOnly(2026, 8, 20)]);

        Apply(
            progress,
            new DateOnly(2026, 8, 22),
            [new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 22)]);

        Assert.Equal(2, progress.CurrentStreak);
        Assert.Equal(2, progress.LongestStreak);
    }

    [Fact]
    public void ApplyAttempt_RepeatedPublishedDay_IsANoOp()
    {
        var progress = CreateProgress();
        var publishDate = new DateOnly(2026, 8, 20);
        Apply(progress, publishDate, [publishDate]);
        var originalUpdatedAt = progress.UpdatedAt;

        progress.ApplyAttempt(
            AttemptScoring.Calculate(isCorrect: true),
            publishDate,
            [publishDate],
            Now.AddMinutes(5));

        Assert.Equal(10, progress.LifetimeXp);
        Assert.Equal(1, progress.CurrentStreak);
        Assert.Equal(1, progress.LongestStreak);
        Assert.Equal(publishDate, progress.LastCompletedPublishDate);
        Assert.Equal(originalUpdatedAt, progress.UpdatedAt);
    }

    [Fact]
    public void CorrectAndIncorrectAreEligible()
    {
        var correct = CreateProgress();
        var incorrect = CreateProgress();
        var publishDate = new DateOnly(2026, 8, 20);

        correct.ApplyAttempt(
            AttemptScoring.Calculate(isCorrect: true),
            publishDate,
            [publishDate],
            Now);
        incorrect.ApplyAttempt(
            AttemptScoring.Calculate(isCorrect: false),
            publishDate,
            [publishDate],
            Now);

        Assert.Equal(15, correct.LifetimeXp);
        Assert.Equal(10, incorrect.LifetimeXp);
        Assert.Equal(correct.CurrentStreak, incorrect.CurrentStreak);
        Assert.Equal(correct.LongestStreak, incorrect.LongestStreak);
        Assert.Equal(correct.LastCompletedPublishDate, incorrect.LastCompletedPublishDate);
    }

    [Fact]
    public void LongestStreakNeverDecreases()
    {
        var progress = CreateProgress();
        Apply(progress, new DateOnly(2026, 8, 20), [new DateOnly(2026, 8, 20)]);
        Apply(
            progress,
            new DateOnly(2026, 8, 21),
            [new DateOnly(2026, 8, 20), new DateOnly(2026, 8, 21)]);
        Apply(
            progress,
            new DateOnly(2026, 8, 22),
            [new DateOnly(2026, 8, 21), new DateOnly(2026, 8, 22)]);

        Apply(
            progress,
            new DateOnly(2026, 8, 24),
            [
                new DateOnly(2026, 8, 22),
                new DateOnly(2026, 8, 23),
                new DateOnly(2026, 8, 24),
            ]);

        Assert.Equal(1, progress.CurrentStreak);
        Assert.Equal(3, progress.LongestStreak);
    }

    private static UserProgress CreateProgress() => new(Guid.NewGuid(), Now.AddDays(-10));

    private static void Apply(
        UserProgress progress,
        DateOnly publishDate,
        IReadOnlyCollection<DateOnly> publishedDates) => progress.ApplyAttempt(
            AttemptScoring.Calculate(isCorrect: false),
            publishDate,
            publishedDates,
            Now);
}
