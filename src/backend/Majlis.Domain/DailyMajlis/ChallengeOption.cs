namespace Majlis.Domain.DailyMajlis;

public sealed class ChallengeOption
{
    public ChallengeOption(Guid id, string text, bool isCorrect, int sortOrder)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A challenge option id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Challenge option text is required.", nameof(text));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");
        }

        Id = id;
        Text = text;
        IsCorrect = isCorrect;
        SortOrder = sortOrder;
    }

    public Guid Id { get; }

    public string Text { get; }

    public bool IsCorrect { get; }

    public int SortOrder { get; }
}
