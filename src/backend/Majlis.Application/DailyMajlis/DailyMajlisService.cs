using System.Globalization;
using Majlis.Contracts.DailyMajlis;
using Majlis.Domain.DailyMajlis;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Application.DailyMajlis;

public sealed class DailyMajlisService(
    IDailyMajlisRepository repository,
    TimeProvider timeProvider) : IDailyMajlisService
{
    public async Task<DailyMajlisResponse?> GetTodayAsync(
        CancellationToken cancellationToken = default)
    {
        var localized = await GetTodayAsync("ar", cancellationToken);
        return localized?.Response;
    }

    public async Task<LocalizedDailyMajlisResponse?> GetTodayAsync(
        string? acceptLanguage,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var dailyMajlis = await repository.GetPublishedByDateAsync(today, cancellationToken);
        var revision = dailyMajlis?.PublishedRevision;

        if (dailyMajlis is null || revision is null ||
            !revision.IsImmutable || !revision.IsCompleteForServing())
        {
            return null;
        }

        var locale = LocaleSelector.Select(acceptLanguage, revision);
        var translation = revision.Translations.SingleOrDefault(item => item.Locale == locale);
        if (translation is null)
        {
            return null;
        }

        return new LocalizedDailyMajlisResponse(
            MapResponse(dailyMajlis, revision, translation),
            locale);
    }

    private static DailyMajlisResponse MapResponse(
        DailyMajlisEntity dailyMajlis,
        DailyMajlisRevision revision,
        DailyMajlisTranslation translation)
    {
        var challenge = revision.Challenge!;

        return new DailyMajlisResponse(
            dailyMajlis.Id,
            dailyMajlis.PublishDate,
            translation.Title,
            revision.TopicCode,
            new DailyMajlisChallengeResponse(
                challenge.Id,
                translation.QuestionText,
                MapType(challenge.Type),
                MapDifficulty(revision.Difficulty),
                revision.Regions.FirstOrDefault()?.RegionCode,
                challenge.Options
                    .OrderBy(option => option.SortOrder)
                    .Select(option => new ChallengeOptionResponse(
                        option.Id,
                        option.Translations.Single(item => item.Locale == translation.Locale).Text))
                    .ToArray()),
            translation.DiscussionPrompt,
            new DailyMajlisUserStateResponse(HasAttempted: false, AttemptId: null));
    }

    private static string MapType(ChallengeType type) => type switch
    {
        ChallengeType.MultipleChoice => "multiple_choice",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown challenge type."),
    };

    private static string MapDifficulty(ChallengeDifficulty difficulty) => difficulty switch
    {
        ChallengeDifficulty.Easy => "easy",
        ChallengeDifficulty.Medium => "medium",
        ChallengeDifficulty.Hard => "hard",
        _ => throw new ArgumentOutOfRangeException(
            nameof(difficulty),
            difficulty,
            "Unknown challenge difficulty."),
    };
}

public sealed record LocalizedDailyMajlisResponse(
    DailyMajlisResponse Response,
    string ContentLanguage);

internal static class LocaleSelector
{
    public static string Select(string? acceptLanguage, DailyMajlisRevision revision)
    {
        var available = revision.Translations
            .Where(translation => translation.IsComplete && revision.Challenge!.Options.All(option =>
                option.Translations.Any(item => item.Locale == translation.Locale && !string.IsNullOrWhiteSpace(item.Text))))
            .Select(translation => translation.Locale)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var locale in Parse(acceptLanguage))
        {
            if (available.Contains(locale))
            {
                return available.First(item => string.Equals(item, locale, StringComparison.OrdinalIgnoreCase));
            }

            var baseLocale = locale.Split('-', 2)[0];
            if (available.Contains(baseLocale))
            {
                return available.First(item => string.Equals(item, baseLocale, StringComparison.OrdinalIgnoreCase));
            }
        }

        return available.FirstOrDefault(item => string.Equals(item, "ar", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("A servable revision must include Arabic.");
    }

    private static IEnumerable<string> Parse(string? acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            yield return "ar";
            yield break;
        }

        var values = acceptLanguage.Split(',')
            .Select((value, index) =>
            {
                var parts = value.Split(';');
                var locale = parts[0].Trim().ToLowerInvariant();
                var quality = parts.Skip(1)
                    .Select(part => part.Trim())
                    .Where(part => part.StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                    .Select(part => double.TryParse(part[2..], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 1d)
                    .DefaultIfEmpty(1d)
                    .First();
                return (locale, quality, index);
            })
            .Where(item => item.quality > 0 && !string.IsNullOrWhiteSpace(item.locale) && item.locale != "*")
            .OrderByDescending(item => item.quality)
            .ThenBy(item => item.index);
        foreach (var value in values)
        {
            yield return value.locale;
        }
    }
}
