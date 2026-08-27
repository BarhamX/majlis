using Majlis.Application.DailyMajlis;
using Majlis.Domain.DailyMajlis;

namespace Majlis.Tests.Application;

public sealed class DailyMajlisLocalizationTests
{
    [Theory]
    [InlineData("ar-QA", "ar")]
    [InlineData("ar", "ar")]
    [InlineData("en-US", "en")]
    [InlineData("en;q=0,fr;q=0.9", "ar")]
    [InlineData("fr", "ar")]
    public async Task GetTodayAsync_SelectsLocaleAndReportsContentLanguage(
        string acceptLanguage,
        string expectedLocale)
    {
        var revision = CreateRevision();
        var dailyMajlisId = revision.DailyMajlisId;
        var dailyMajlis = new DailyMajlis(
            dailyMajlisId,
            new DateOnly(2026, 8, 26),
            DailyMajlisStatus.Published,
            revision);
        var service = new DailyMajlisService(
            new StubDailyMajlisRepository(dailyMajlis),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetTodayAsync(acceptLanguage);

        Assert.NotNull(result);
        Assert.Equal(expectedLocale, result.ContentLanguage);
        Assert.Equal(expectedLocale == "ar" ? "العنوان" : "Title", result.Response.Title);
    }

    private static DailyMajlisRevision CreateRevision()
    {
        var revisionId = Guid.NewGuid();
        var revision = new DailyMajlisRevision(
            revisionId,
            Guid.NewGuid(),
            1,
            "hospitality",
            ChallengeDifficulty.Easy,
            CardType.Proverb,
            "Verified source notes.",
            null,
            DateTimeOffset.UtcNow);
        revision.SetChallenge(new Challenge(
            Guid.NewGuid(),
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.NewGuid(), "A", true, 1),
                new ChallengeOption(Guid.NewGuid(), "B", false, 2),
            ]));
        revision.AddTranslation(new DailyMajlisTranslation(revision.Id, "ar", "العنوان", "السؤال", "الشرح", "النقاش", "البطاقة"));
        revision.AddTranslation(new DailyMajlisTranslation(revision.Id, "en", "Title", "Question", "Explanation", "Discussion", "Card"));
        foreach (var option in revision.Challenge!.Options)
        {
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "ar", "خيار"));
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "en", "Option"));
        }

        return revision;
    }

    private sealed class StubDailyMajlisRepository(DailyMajlis? dailyMajlis) : IDailyMajlisRepository
    {
        public Task<DailyMajlis?> GetPublishedByDateAsync(DateOnly publishDate, CancellationToken cancellationToken = default)
            => Task.FromResult(dailyMajlis);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
