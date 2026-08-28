namespace Majlis.Contracts.Identity;

public sealed record BootstrapProfileRequest(
    string DisplayName,
    string AgeBand,
    string? CountryCode,
    string? RegionCode,
    string? DialectCode,
    string Locale,
    string AcceptedTermsVersion,
    string AcceptedPrivacyVersion);

public sealed record UpdateProfileRequest(
    string DisplayName,
    string AgeBand,
    string? CountryCode,
    string? RegionCode,
    string? DialectCode,
    string Locale,
    string LeaderboardVisibility);

public sealed record UserProfileResponse(
    Guid UserId,
    bool ProfileComplete,
    string DisplayName,
    string AgeBand,
    string? CountryCode,
    string? RegionCode,
    string? DialectCode,
    string Locale,
    string LeaderboardVisibility,
    UserPreferencesResponse Preferences,
    IReadOnlyList<string> LinkedProviders,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UserPreferencesResponse(
    bool ReminderEnabled,
    string? ReminderLocalTime,
    string? ReminderTimeZoneId,
    bool AnalyticsConsent);

public sealed record AccountDeletionRequest(string Confirmation);

public sealed record AccountDeletionResponse(
    Guid RequestId,
    DateTimeOffset RequestedAt,
    DateTimeOffset PurgeDueAt);

public sealed record TestAccessTokenRequest(string Subject);

public sealed record TestAccessTokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt);
