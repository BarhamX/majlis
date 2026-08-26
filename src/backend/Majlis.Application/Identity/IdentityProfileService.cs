using Majlis.Contracts.Identity;
using Majlis.Domain.Identity;
using AccountDeletionContract = Majlis.Contracts.Identity.AccountDeletionRequest;

namespace Majlis.Application.Identity;

public sealed class IdentityProfileService(
    IUserAccountRepository userAccountRepository,
    TimeProvider timeProvider) : IIdentityProfileService
{
    public async Task<(UserProfileResponse Profile, bool Created)> BootstrapAsync(
        AuthenticatedIdentity identity,
        BootstrapProfileRequest request,
        CancellationToken cancellationToken)
    {
        var ageBand = ParseAgeBand(request.AgeBand);

        var now = timeProvider.GetUtcNow();
        var user = await userAccountRepository.FindByIdentityAsync(
            identity.Provider,
            identity.Issuer,
            identity.Subject,
            cancellationToken);
        bool created;

        try
        {
            if (user is null)
            {
                created = true;
                user = UserAccount.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    identity.Provider,
                    identity.Issuer,
                    identity.Subject,
                    now);
                user.CompleteProfile(
                    request.DisplayName,
                    ageBand,
                    request.CountryCode,
                    request.RegionCode,
                    request.DialectCode,
                    request.Locale,
                    now);
                userAccountRepository.Add(user);
            }
            else
            {
                created = false;
                EnsureActiveSession(user, identity);
                var linkedIdentity = user.Identities.Single(item =>
                    item.Provider == identity.Provider &&
                    item.Issuer == identity.Issuer &&
                    item.Subject == identity.Subject);
                user.MarkAuthenticated(linkedIdentity, now);
                user.CompleteProfile(
                    request.DisplayName,
                    ageBand,
                    request.CountryCode,
                    request.RegionCode,
                    request.DialectCode,
                    request.Locale,
                    now);
            }

            user.RecordRequiredConsents(
                Guid.NewGuid(),
                request.AcceptedTermsVersion,
                Guid.NewGuid(),
                request.AcceptedPrivacyVersion,
                now);
        }
        catch (ArgumentException exception)
        {
            throw ValidationFailed(exception);
        }

        try
        {
            await userAccountRepository.SaveChangesAsync(cancellationToken);
        }
        catch (IdentityConflictException) when (created)
        {
            user = await userAccountRepository.FindByIdentityAsync(
                identity.Provider,
                identity.Issuer,
                identity.Subject,
                cancellationToken);
            if (user is null)
            {
                throw;
            }

            EnsureActiveSession(user, identity);
            created = false;
        }

        return (Map(user), created);
    }

    public async Task<UserProfileResponse> GetProfileAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(identity, cancellationToken);
        return Map(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(
        AuthenticatedIdentity identity,
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(identity, cancellationToken);
        var profile = user.Profile ?? throw ProfileIncomplete();
        var ageBand = ParseAgeBand(request.AgeBand);
        var visibility = ParseLeaderboardVisibility(request.LeaderboardVisibility);
        var now = timeProvider.GetUtcNow();

        try
        {
            profile.Update(
                request.DisplayName,
                ageBand,
                request.CountryCode,
                request.RegionCode,
                request.DialectCode,
                request.Locale,
                visibility,
                now);
        }
        catch (InvalidOperationException exception)
        {
            throw new IdentityProfileException(
                "leaderboard_age_ineligible",
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            throw ValidationFailed(exception);
        }

        await userAccountRepository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task RevokeAllSessionsAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(identity, cancellationToken);
        user.RevokeAuthentication(timeProvider.GetUtcNow());
        await userAccountRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AccountDeletionResponse> RequestDeletionAsync(
        AuthenticatedIdentity identity,
        AccountDeletionContract request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(request.Confirmation, "delete_my_account", StringComparison.Ordinal))
        {
            throw new IdentityProfileException(
                "validation_failed",
                "Account deletion requires explicit confirmation.");
        }

        var user = await GetActiveUserAsync(identity, cancellationToken);
        var deletion = user.RequestDeletion(Guid.NewGuid(), timeProvider.GetUtcNow());
        await userAccountRepository.SaveChangesAsync(cancellationToken);

        return new AccountDeletionResponse(deletion.Id, deletion.RequestedAt, deletion.PurgeDueAt);
    }

    private async Task<UserAccount> GetActiveUserAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.FindByIdentityAsync(
            identity.Provider,
            identity.Issuer,
            identity.Subject,
            cancellationToken);
        if (user is null || user.Profile is null)
        {
            throw ProfileIncomplete();
        }

        EnsureActiveSession(user, identity);
        return user;
    }

    private static void EnsureActiveSession(UserAccount user, AuthenticatedIdentity identity)
    {
        if (user.Status != UserAccountStatus.Active ||
            (user.AuthenticationNotBefore.HasValue &&
             identity.IssuedAt <= user.AuthenticationNotBefore.Value))
        {
            throw new IdentityProfileException(
                "authentication_required",
                "The Majlis session is no longer active.");
        }
    }

    private static AgeBand ParseAgeBand(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "13_17" => AgeBand.Minor13To17,
        "18_plus" => AgeBand.Adult18Plus,
        "under_13" => throw new IdentityProfileException(
            "age_not_eligible",
            "Majlis accounts are available only to users aged 13 or older."),
        _ => throw new IdentityProfileException(
            "validation_failed",
            "Age band must be 13_17 or 18_plus."),
    };

    private static LeaderboardVisibility ParseLeaderboardVisibility(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "private" => LeaderboardVisibility.Private,
            "global_weekly" => LeaderboardVisibility.GlobalWeekly,
            _ => throw new IdentityProfileException(
                "validation_failed",
                "Leaderboard visibility must be private or global_weekly."),
        };

    private static UserProfileResponse Map(UserAccount user)
    {
        var profile = user.Profile ?? throw ProfileIncomplete();
        return new UserProfileResponse(
            user.Id,
            ProfileComplete: true,
            profile.DisplayName,
            ToContract(profile.AgeBand),
            profile.CountryCode,
            profile.RegionCode,
            profile.DialectCode,
            profile.Locale,
            ToContract(profile.LeaderboardVisibility),
            new UserPreferencesResponse(
                user.Preferences.ReminderEnabled,
                user.Preferences.ReminderLocalTime?.ToString("HH:mm"),
                user.Preferences.ReminderTimeZoneId,
                user.Preferences.AnalyticsConsent),
            user.Identities
                .Select(identity => ToContract(identity.Provider))
                .Order(StringComparer.Ordinal)
                .ToArray(),
            profile.CreatedAt,
            profile.UpdatedAt);
    }

    private static string ToContract(AgeBand value) => value switch
    {
        AgeBand.Minor13To17 => "13_17",
        AgeBand.Adult18Plus => "18_plus",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    private static string ToContract(LeaderboardVisibility value) => value switch
    {
        LeaderboardVisibility.Private => "private",
        LeaderboardVisibility.GlobalWeekly => "global_weekly",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    private static string ToContract(ExternalIdentityProvider value) => value switch
    {
        ExternalIdentityProvider.Google => "google",
        ExternalIdentityProvider.Apple => "apple",
        ExternalIdentityProvider.Meta => "meta",
        ExternalIdentityProvider.Snapchat => "snapchat",
        ExternalIdentityProvider.Test => "test",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    private static IdentityProfileException ProfileIncomplete() => new(
        "profile_incomplete",
        "Complete the Majlis profile before continuing.");

    private static IdentityProfileException ValidationFailed(ArgumentException exception) => new(
        "validation_failed",
        exception.Message);
}
