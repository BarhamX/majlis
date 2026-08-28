namespace Majlis.Domain.Progress;

public sealed class IdempotencyRecord
{
    private IdempotencyRecord()
    {
        Scope = string.Empty;
        RequestHash = string.Empty;
    }

    public IdempotencyRecord(
        Guid userId,
        string scope,
        Guid idempotencyKey,
        string requestHash,
        Guid? resourceId,
        int responseStatus,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        UserId = RequireId(userId, nameof(userId));
        Scope = RequireText(scope, nameof(scope));
        IdempotencyKey = RequireId(idempotencyKey, nameof(idempotencyKey));
        RequestHash = RequireText(requestHash, nameof(requestHash));
        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("A resource id must be non-empty when supplied.", nameof(resourceId));
        }

        if (responseStatus is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(responseStatus));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt));
        }

        ResourceId = resourceId;
        ResponseStatus = responseStatus;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private set; }

    public string Scope { get; private set; }

    public Guid IdempotencyKey { get; private set; }

    public string RequestHash { get; private set; }

    public Guid? ResourceId { get; private set; }

    public int ResponseStatus { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    private static Guid RequireId(Guid value, string parameterName) => value == Guid.Empty
        ? throw new ArgumentException("An id is required.", parameterName)
        : value;

    private static string RequireText(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
}
