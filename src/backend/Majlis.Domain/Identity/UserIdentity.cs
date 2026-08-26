namespace Majlis.Domain.Identity;

public sealed class UserIdentity
{
    private UserIdentity()
    {
        Issuer = string.Empty;
        Subject = string.Empty;
    }

    internal UserIdentity(
        Guid id,
        Guid userId,
        ExternalIdentityProvider provider,
        string issuer,
        string subject,
        DateTimeOffset linkedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An identity id is required.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A user id is required.", nameof(userId));
        }

        Id = id;
        UserId = userId;
        Provider = provider;
        Issuer = RequireText(issuer, nameof(issuer));
        Subject = RequireText(subject, nameof(subject));
        LinkedAt = linkedAt;
        LastAuthenticatedAt = linkedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ExternalIdentityProvider Provider { get; private set; }

    public string Issuer { get; private set; }

    public string Subject { get; private set; }

    public byte[]? RevocationHandleCiphertext { get; private set; }

    public string? RevocationKeyVersion { get; private set; }

    public DateTimeOffset LinkedAt { get; private set; }

    public DateTimeOffset LastAuthenticatedAt { get; private set; }

    public DateTimeOffset? ProviderAuthorizationRevokedAt { get; private set; }

    internal void MarkAuthenticated(DateTimeOffset now) => LastAuthenticatedAt = now;

    private static string RequireText(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
}
