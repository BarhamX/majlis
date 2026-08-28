using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Majlis.Domain.Identity;

public sealed partial class UserProfile
{
    private const int MinimumDisplayNameLength = 3;
    private const int MaximumDisplayNameLength = 30;
    private static readonly HashSet<string> KnownCountryCodes = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(culture => new RegionInfo(culture.Name).TwoLetterISORegionName)
        .ToHashSet(StringComparer.Ordinal);

    private UserProfile()
    {
    }

    internal UserProfile(
        Guid userId,
        string displayName,
        AgeBand ageBand,
        string? countryCode,
        string? regionCode,
        string? dialectCode,
        string locale,
        DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A user id is required.", nameof(userId));
        }

        UserId = userId;
        AgeBand = ageBand;
        SetValues(displayName, countryCode, regionCode, dialectCode, locale);
        LeaderboardVisibility = LeaderboardVisibility.Private;
        AgeBandAttestedAt = now;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string DisplayNameNormalized { get; private set; } = string.Empty;

    public AgeBand AgeBand { get; private set; }

    public DateTimeOffset AgeBandAttestedAt { get; private set; }

    public string? CountryCode { get; private set; }

    public string? RegionCode { get; private set; }

    public string? DialectCode { get; private set; }

    public string Locale { get; private set; } = "ar";

    public LeaderboardVisibility LeaderboardVisibility { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string displayName,
        AgeBand ageBand,
        string? countryCode,
        string? regionCode,
        string? dialectCode,
        string locale,
        LeaderboardVisibility leaderboardVisibility,
        DateTimeOffset now)
    {
        if (AgeBand != ageBand)
        {
            AgeBand = ageBand;
            AgeBandAttestedAt = now;
        }

        SetValues(displayName, countryCode, regionCode, dialectCode, locale);
        SetLeaderboardVisibility(leaderboardVisibility, now);
        UpdatedAt = now;
    }

    public void SetLeaderboardVisibility(
        LeaderboardVisibility visibility,
        DateTimeOffset now)
    {
        if (visibility == LeaderboardVisibility.GlobalWeekly && AgeBand != AgeBand.Adult18Plus)
        {
            throw new InvalidOperationException(
                "A user must be 18 or older to join the global weekly leaderboard.");
        }

        LeaderboardVisibility = visibility;
        UpdatedAt = now;
    }

    private void SetValues(
        string displayName,
        string? countryCode,
        string? regionCode,
        string? dialectCode,
        string locale)
    {
        var normalizedDisplayName = RequireDisplayName(displayName);
        DisplayName = normalizedDisplayName;
        DisplayNameNormalized = normalizedDisplayName.ToUpperInvariant();
        CountryCode = NormalizeCountryCode(countryCode);
        RegionCode = NormalizeOptionalCode(regionCode, nameof(regionCode));
        DialectCode = NormalizeOptionalCode(dialectCode, nameof(dialectCode));
        Locale = NormalizeLocale(locale);
    }

    private static string RequireDisplayName(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Normalize(NormalizationForm.FormC);
        var textElements = StringInfo.ParseCombiningCharacters(normalized).Length;
        if (textElements is < MinimumDisplayNameLength or > MaximumDisplayNameLength)
        {
            throw new ArgumentException(
                "Display name must contain between 3 and 30 characters.",
                nameof(value));
        }

        if (normalized.Any(char.IsControl))
        {
            throw new ArgumentException("Display name cannot contain control characters.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeCountryCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (!CountryCodePattern().IsMatch(normalized) || !KnownCountryCodes.Contains(normalized))
        {
            throw new ArgumentException(
                "Country code must be an ISO 3166-1 alpha-2 value.",
                nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeOptionalCode(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!ProfileCodePattern().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Profile code must use lowercase letters, numbers, and underscores.",
                parameterName);
        }

        return normalized;
    }

    private static string NormalizeLocale(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A locale is required.", nameof(value));
        }

        try
        {
            return CultureInfo.GetCultureInfo(value.Trim()).Name;
        }
        catch (CultureNotFoundException exception)
        {
            throw new ArgumentException("Locale must be a valid BCP 47 language tag.", nameof(value), exception);
        }
    }

    [GeneratedRegex("^[A-Z]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex CountryCodePattern();

    [GeneratedRegex("^[a-z0-9_]{2,32}$", RegexOptions.CultureInvariant)]
    private static partial Regex ProfileCodePattern();
}
