namespace Majlis.Domain.DailyMajlis;

public sealed class ChallengeOption
{
    private readonly List<ChallengeOptionTranslation> _translations = [];

    private ChallengeOption()
    {
        OptionKey = string.Empty;
    }

    public ChallengeOption(Guid id, string optionKey, bool isCorrect, int sortOrder)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A challenge option id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(optionKey))
        {
            throw new ArgumentException("Challenge option key is required.", nameof(optionKey));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");
        }

        Id = id;
        OptionKey = optionKey.Trim();
        IsCorrect = isCorrect;
        SortOrder = sortOrder;
    }

    public Guid Id { get; private set; }

    public string OptionKey { get; private set; }

    public bool IsCorrect { get; private set; }

    public int SortOrder { get; private set; }

    public IReadOnlyList<ChallengeOptionTranslation> Translations => _translations;

    internal void AddTranslation(ChallengeOptionTranslation translation)
    {
        ArgumentNullException.ThrowIfNull(translation);
        if (translation.OptionId != Id)
        {
            throw new ArgumentException("Translation belongs to another option.", nameof(translation));
        }

        _translations.RemoveAll(existing => existing.Locale == translation.Locale);
        _translations.Add(translation);
    }
}
