namespace Majlis.Domain.Progress;

public sealed class XpLedgerEntry
{
    private XpLedgerEntry()
    {
    }

    public XpLedgerEntry(
        Guid id,
        Guid userId,
        Guid attemptId,
        int amount,
        DateTimeOffset occurredAt)
    {
        Id = RequireId(id, nameof(id));
        UserId = RequireId(userId, nameof(userId));
        AttemptId = RequireId(attemptId, nameof(attemptId));
        if (amount is not 10 and not 15)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "An XP award must be exactly 10 or 15.");
        }

        Amount = amount;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid AttemptId { get; private set; }

    public int Amount { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    private static Guid RequireId(Guid value, string parameterName) => value == Guid.Empty
        ? throw new ArgumentException("An id is required.", parameterName)
        : value;
}
