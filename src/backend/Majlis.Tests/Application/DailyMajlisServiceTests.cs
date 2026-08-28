using System.Text.Json;
using Majlis.Application.DailyMajlis;
using Majlis.Domain.DailyMajlis;

namespace Majlis.Tests.Application;

public sealed class DailyMajlisServiceTests
{
    [Fact]
    public async Task GetTodayAsync_UsesUtcCalendarDate()
    {
        var repository = new StubDailyMajlisRepository(dailyMajlis: null);
        var service = new DailyMajlisService(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 23, 59, 0, TimeSpan.Zero)));

        var result = await service.GetTodayAsync();

        Assert.Null(result);
        Assert.Equal(new DateOnly(2026, 8, 26), repository.RequestedDate);
    }

    [Fact]
    public async Task GetTodayAsync_WhenPublishedMajlisExists_ReturnsValidSpoilerSafePayload()
    {
        var today = new DateOnly(2026, 8, 26);
        var challengeId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var revisionId = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var challenge = new Challenge(
            challengeId,
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.Parse("30000000-0000-0000-0000-000000000001"), "A guest is honored as a trust", true, 1),
                new ChallengeOption(Guid.Parse("30000000-0000-0000-0000-000000000002"), "A guest should not stay long", false, 2),
            ]);
        var revision = new DailyMajlisRevision(
            revisionId,
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            1,
            "hospitality",
            ChallengeDifficulty.Easy,
            CardType.Proverb,
            "Verified source notes.",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
        revision.SetChallenge(challenge);
        revision.AddTranslation(new DailyMajlisTranslation(revision.Id, "ar", "العنوان", "السؤال", "الشرح", "النقاش", "البطاقة"));
        foreach (var option in challenge.Options)
        {
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "ar", "خيار"));
        }
        revision.Submit(DateTimeOffset.UtcNow);
        var dailyMajlis = new DailyMajlis(
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            today,
            DailyMajlisStatus.Published,
            revision);
        var repository = new StubDailyMajlisRepository(dailyMajlis);
        var service = new DailyMajlisService(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetTodayAsync();

        Assert.NotNull(result);
        Assert.Equal(dailyMajlis.Id, result.DailyMajlisId);
        Assert.Equal(today, result.PublishDate);
        Assert.Equal(challengeId, result.Challenge.Id);
        Assert.Equal(2, result.Challenge.Options.Count);
        Assert.All(result.Challenge.Options, option =>
        {
            Assert.NotEqual(Guid.Empty, option.Id);
            Assert.False(string.IsNullOrWhiteSpace(option.Text));
        });
        Assert.Equal(today, repository.RequestedDate);
        var serializedResponse = JsonSerializer.Serialize(result);
        Assert.DoesNotContain("correct", serializedResponse, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", serializedResponse, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetTodayAsync_WhenPublishedRevisionIsMutable_ReturnsUnavailable()
    {
        var revisionId = Guid.NewGuid();
        var dailyMajlisId = Guid.NewGuid();
        var challenge = new Challenge(
            Guid.NewGuid(),
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.NewGuid(), "A", true, 1),
                new ChallengeOption(Guid.NewGuid(), "B", false, 2),
            ]);
        var revision = new DailyMajlisRevision(
            revisionId,
            dailyMajlisId,
            1,
            "hospitality",
            ChallengeDifficulty.Easy,
            CardType.Proverb,
            "Verified source notes.",
            null,
            DateTimeOffset.UtcNow);
        revision.SetChallenge(challenge);
        revision.AddTranslation(new DailyMajlisTranslation(
            revision.Id, "ar", "العنوان", "السؤال", "الشرح", "النقاش", "البطاقة"));
        foreach (var option in challenge.Options)
        {
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "ar", "خيار"));
        }
        revision.Submit(DateTimeOffset.UtcNow);

        var dailyMajlis = new DailyMajlis(
            dailyMajlisId,
            new DateOnly(2026, 8, 26),
            DailyMajlisStatus.Published,
            revision);
        typeof(DailyMajlisRevision)
            .GetProperty(nameof(DailyMajlisRevision.SubmittedAt))!
            .SetValue(revision, null);
        var service = new DailyMajlisService(
            new StubDailyMajlisRepository(dailyMajlis),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero)));

        Assert.Null(await service.GetTodayAsync());
    }

    private sealed class StubDailyMajlisRepository(DailyMajlis? dailyMajlis)
        : IDailyMajlisRepository
    {
        public DateOnly? RequestedDate { get; private set; }

        public Task<DailyMajlis?> GetPublishedByDateAsync(
            DateOnly publishDate,
            CancellationToken cancellationToken = default)
        {
            RequestedDate = publishDate;
            return Task.FromResult<DailyMajlis?>(dailyMajlis);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
