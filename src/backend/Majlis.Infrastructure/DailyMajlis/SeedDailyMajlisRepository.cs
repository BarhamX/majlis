using Majlis.Application.DailyMajlis;
using Majlis.Domain.DailyMajlis;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.DailyMajlis;

public sealed class SeedDailyMajlisRepository : IDailyMajlisRepository
{
    public Task<DailyMajlisEntity?> GetPublishedByDateAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DailyMajlisEntity dailyMajlis = new(
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            publishDate,
            "The Guest Before the House",
            "hospitality",
            new Challenge(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "What does this proverb mean?",
                ChallengeType.MultipleChoice,
                ChallengeDifficulty.Easy,
                "panArab",
                "hospitality",
                "This proverb reflects hospitality as honor and responsibility.",
                "Seed content from docs/architecture/API_CONTRACTS.md; replace before production.",
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

        return Task.FromResult<DailyMajlisEntity?>(dailyMajlis);
    }
}
