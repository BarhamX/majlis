namespace Majlis.Domain.DailyMajlis;

public sealed class Challenge
{
    private readonly List<ChallengeOption> _options = [];

    private Challenge()
    {
    }

    public Challenge(
        Guid id,
        Guid revisionId,
        ChallengeType type,
        IEnumerable<ChallengeOption> options)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A challenge id is required.", nameof(id));
        }

        if (revisionId == Guid.Empty)
        {
            throw new ArgumentException("A revision id is required.", nameof(revisionId));
        }

        ArgumentNullException.ThrowIfNull(options);
        var orderedOptions = options.OrderBy(option => option.SortOrder).ToArray();
        if (orderedOptions.Length is < 2 or > 4)
        {
            throw new ArgumentException("A challenge requires between two and four options.", nameof(options));
        }

        if (type == ChallengeType.MultipleChoice && orderedOptions.Count(option => option.IsCorrect) != 1)
        {
            throw new ArgumentException(
                "A multiple-choice challenge requires exactly one correct option.",
                nameof(options));
        }

        Id = id;
        RevisionId = revisionId;
        Type = type;
        _options.AddRange(orderedOptions);
    }

    public Guid Id { get; private set; }

    public Guid RevisionId { get; private set; }

    public ChallengeType Type { get; private set; }

    public IReadOnlyList<ChallengeOption> Options => _options;
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

public enum CardType
{
    Proverb,
    Story,
    Saying,
    Tradition,
}
