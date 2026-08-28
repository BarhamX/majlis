namespace Majlis.Domain.Identity;

public sealed class UserConsent
{
    private UserConsent()
    {
        Version = string.Empty;
    }

    internal UserConsent(
        Guid id,
        Guid userId,
        ConsentType type,
        string version,
        bool accepted,
        DateTimeOffset recordedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A consent id is required.", nameof(id));
        }

        Id = id;
        UserId = userId;
        Type = type;
        Version = string.IsNullOrWhiteSpace(version)
            ? throw new ArgumentException("A consent version is required.", nameof(version))
            : version.Trim();
        Accepted = accepted;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ConsentType Type { get; private set; }

    public string Version { get; private set; }

    public bool Accepted { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }
}
