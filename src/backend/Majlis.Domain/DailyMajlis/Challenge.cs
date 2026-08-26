namespace Majlis.Domain.DailyMajlis;

public sealed class Challenge
{
    public Challenge(
        Guid id,
        string questionText,
        ChallengeType type,
        ChallengeDifficulty difficulty,
        string? region,
        string topic,
        string explanation,
        string? sourceNotes,
        ContentReviewStatus reviewStatus,
        IEnumerable<ChallengeOption> options)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A challenge id is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(options);

        var orderedOptions = options.OrderBy(option => option.SortOrder).ToArray();
        if (orderedOptions.Length == 0)
        {
            throw new ArgumentException("A challenge requires at least one option.", nameof(options));
        }

        if (type == ChallengeType.MultipleChoice && orderedOptions.Count(option => option.IsCorrect) != 1)
        {
            throw new ArgumentException(
                "A multiple-choice challenge requires exactly one correct option.",
                nameof(options));
        }

        Id = id;
        QuestionText = RequireText(questionText, nameof(questionText));
        Type = type;
        Difficulty = difficulty;
        Region = string.IsNullOrWhiteSpace(region) ? null : region;
        Topic = RequireText(topic, nameof(topic));
        Explanation = RequireText(explanation, nameof(explanation));
        SourceNotes = string.IsNullOrWhiteSpace(sourceNotes) ? null : sourceNotes;
        ReviewStatus = reviewStatus;
        Options = Array.AsReadOnly(orderedOptions);
    }

    public Guid Id { get; }

    public string QuestionText { get; }

    public ChallengeType Type { get; }

    public ChallengeDifficulty Difficulty { get; }

    public string? Region { get; }

    public string Topic { get; }

    public string Explanation { get; }

    public string? SourceNotes { get; }

    public ContentReviewStatus ReviewStatus { get; }

    public IReadOnlyList<ChallengeOption> Options { get; }

    private static string RequireText(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value;
    }
}

public enum ChallengeType
{
    MultipleChoice,
}

public enum ChallengeDifficulty
{
    Easy,
    Medium,
    Hard,
}

public enum ContentReviewStatus
{
    Draft,
    Reviewed,
    Rejected,
}
