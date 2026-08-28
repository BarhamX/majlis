namespace Majlis.Domain.Progress;

public sealed class UserAttempt
{
    private UserAttempt()
    {
        ResultLocale = string.Empty;
    }

    public UserAttempt(
        Guid id,
        Guid userId,
        Guid dailyMajlisId,
        Guid challengeId,
        Guid contentRevisionId,
        Guid selectedOptionId,
        bool isCorrect,
        AttemptScore score,
        string resultLocale,
        long lifetimeXpAfter,
        int currentStreakAfter,
        int longestStreakAfter,
        DateTimeOffset attemptedAt)
    {
        Id = RequireId(id, nameof(id));
        UserId = RequireId(userId, nameof(userId));
        DailyMajlisId = RequireId(dailyMajlisId, nameof(dailyMajlisId));
        ChallengeId = RequireId(challengeId, nameof(challengeId));
        ContentRevisionId = RequireId(contentRevisionId, nameof(contentRevisionId));
        SelectedOptionId = RequireId(selectedOptionId, nameof(selectedOptionId));
        ArgumentNullException.ThrowIfNull(score);
        var expectedScore = AttemptScoring.Calculate(isCorrect);
        if (score.CompletionXp != expectedScore.CompletionXp ||
            score.CorrectnessXp != expectedScore.CorrectnessXp)
        {
            throw new ArgumentException("The score must match the attempt correctness.", nameof(score));
        }

        if (lifetimeXpAfter < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetimeXpAfter));
        }

        if (currentStreakAfter < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentStreakAfter));
        }

        if (longestStreakAfter < currentStreakAfter)
        {
            throw new ArgumentOutOfRangeException(nameof(longestStreakAfter));
        }

        IsCorrect = isCorrect;
        CompletionXp = score.CompletionXp;
        CorrectnessXp = score.CorrectnessXp;
        ResultLocale = RequireText(resultLocale, nameof(resultLocale)).ToLowerInvariant();
        LifetimeXpAfter = lifetimeXpAfter;
        CurrentStreakAfter = currentStreakAfter;
        LongestStreakAfter = longestStreakAfter;
        AttemptedAt = attemptedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid DailyMajlisId { get; private set; }

    public Guid ChallengeId { get; private set; }

    public Guid ContentRevisionId { get; private set; }

    public Guid SelectedOptionId { get; private set; }

    public bool IsCorrect { get; private set; }

    public int CompletionXp { get; private set; }

    public int CorrectnessXp { get; private set; }

    public string ResultLocale { get; private set; }

    public long LifetimeXpAfter { get; private set; }

    public int CurrentStreakAfter { get; private set; }

    public int LongestStreakAfter { get; private set; }

    public DateTimeOffset AttemptedAt { get; private set; }

    private static Guid RequireId(Guid value, string parameterName) => value == Guid.Empty
        ? throw new ArgumentException("An id is required.", parameterName)
        : value;

    private static string RequireText(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
}
