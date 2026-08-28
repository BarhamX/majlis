namespace Majlis.Domain.Identity;

public sealed class UserAccount
{
    private readonly List<UserIdentity> _identities = [];
    private readonly List<UserConsent> _consents = [];
    private readonly List<UserRoleAssignment> _roleAssignments = [];
    private readonly List<AccountDeletionRequest> _deletionRequests = [];

    private UserAccount()
    {
        Preferences = null!;
    }

    private UserAccount(Guid id, DateTimeOffset now)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A user id is required.", nameof(id));
        }

        Id = id;
        Status = UserAccountStatus.Active;
        CreatedAt = now;
        Preferences = new UserPreferences(id, now);
    }

    public Guid Id { get; private set; }

    public UserAccountStatus Status { get; private set; }

    public DateTimeOffset? AuthenticationNotBefore { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public UserProfile? Profile { get; private set; }

    public UserPreferences Preferences { get; private set; }

    public IReadOnlyList<UserIdentity> Identities => _identities;

    public IReadOnlyList<UserConsent> Consents => _consents;

    public IReadOnlyList<UserRoleAssignment> RoleAssignments => _roleAssignments;

    public IReadOnlyList<AccountDeletionRequest> DeletionRequests => _deletionRequests;

    public static UserAccount Create(
        Guid userId,
        Guid identityId,
        ExternalIdentityProvider provider,
        string issuer,
        string subject,
        DateTimeOffset now)
    {
        var account = new UserAccount(userId, now);
        account.LinkIdentity(identityId, provider, issuer, subject, now);
        account.LastLoginAt = now;
        return account;
    }

    public UserIdentity LinkIdentity(
        Guid identityId,
        ExternalIdentityProvider provider,
        string issuer,
        string subject,
        DateTimeOffset now)
    {
        if (_identities.Any(identity => identity.Provider == provider))
        {
            throw new InvalidOperationException($"A {provider} identity is already linked.");
        }

        var identity = new UserIdentity(identityId, Id, provider, issuer, subject, now);
        _identities.Add(identity);
        return identity;
    }

    public void MarkAuthenticated(UserIdentity identity, DateTimeOffset now)
    {
        if (!_identities.Contains(identity))
        {
            throw new InvalidOperationException("The identity does not belong to this account.");
        }

        identity.MarkAuthenticated(now);
        LastLoginAt = now;
    }

    public UserProfile CompleteProfile(
        string displayName,
        AgeBand ageBand,
        string? countryCode,
        string? regionCode,
        string? dialectCode,
        string locale,
        DateTimeOffset now)
    {
        if (Profile is not null)
        {
            return Profile;
        }

        Profile = new UserProfile(
            Id,
            displayName,
            ageBand,
            countryCode,
            regionCode,
            dialectCode,
            locale,
            now);
        return Profile;
    }

    public void RecordRequiredConsents(
        Guid termsConsentId,
        string termsVersion,
        Guid privacyConsentId,
        string privacyVersion,
        DateTimeOffset now)
    {
        if (_consents.All(consent =>
                consent.Type != ConsentType.Terms || consent.Version != termsVersion))
        {
            _consents.Add(new UserConsent(
                termsConsentId,
                Id,
                ConsentType.Terms,
                termsVersion,
                accepted: true,
                now));
        }

        if (_consents.All(consent =>
                consent.Type != ConsentType.Privacy || consent.Version != privacyVersion))
        {
            _consents.Add(new UserConsent(
                privacyConsentId,
                Id,
                ConsentType.Privacy,
                privacyVersion,
                accepted: true,
                now));
        }
    }

    public void RevokeAuthentication(DateTimeOffset now) => AuthenticationNotBefore = now;

    public AccountDeletionRequest RequestDeletion(Guid requestId, DateTimeOffset now)
    {
        var existing = _deletionRequests.SingleOrDefault(
            request => request.Status != AccountDeletionStatus.Completed);
        if (existing is not null)
        {
            return existing;
        }

        var request = new AccountDeletionRequest(requestId, Id, now);
        _deletionRequests.Add(request);
        Status = UserAccountStatus.DeletionPending;
        AuthenticationNotBefore = now;
        return request;
    }
}
