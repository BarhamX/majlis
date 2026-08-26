using System.Text.Json;
using Majlis.Application.DailyMajlis;
using Majlis.Domain.DailyMajlis;

namespace Majlis.Tests.Application;

public sealed class DailyMajlisServiceTests
{
    [Fact]
    public async Task GetTodayAsync_WhenPublishedMajlisExists_ReturnsValidSpoilerSafePayload()
    {
        var today = new DateOnly(2026, 8, 26);
        var challengeId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var dailyMajlis = new DailyMajlis(
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            today,
            "The Guest Before the House",
            "hospitality",
            new Challenge(
                challengeId,
                "What does this proverb mean?",
                ChallengeType.MultipleChoice,
                ChallengeDifficulty.Easy,
                "panArab",
                "hospitality",
                "This proverb reflects hospitality as honor and responsibility.",
                "Seed content from docs/architecture/API_CONTRACTS.md.",
                ContentReviewStatus.Reviewed,
                [
                    new ChallengeOption(
                        Guid.Parse("30000000-0000-0000-0000-000000000001"),
                        "A guest is honored as a trust",
                        isCorrect: true,
                        sortOrder: 1),
                    new ChallengeOption(
                        Guid.Parse("30000000-0000-0000-0000-000000000002"),
                        "A guest should not stay long",
                        isCorrect: false,
                        sortOrder: 2),
                ]),
            "What is one hospitality habit your family still practices?",
            DailyMajlisStatus.Published);
        var repository = new StubDailyMajlisRepository(dailyMajlis);
        var service = new DailyMajlisService(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetTodayAsync();

        Assert.NotNull(result);
        Assert.Equal(dailyMajlis.Id, result.DailyMajlisId);
        Assert.Equal(today, result.Date);
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

    private sealed class StubDailyMajlisRepository(DailyMajlis dailyMajlis)
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
