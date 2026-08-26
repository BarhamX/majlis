using Majlis.Domain.DailyMajlis;
using Microsoft.EntityFrameworkCore;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.Persistence;

public sealed class DailyMajlisDatabaseInitializer(
    MajlisDbContext dbContext,
    TimeProvider timeProvider)
{
    private static readonly Guid SeedDailyMajlisId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");

    private static readonly Guid SeedChallengeId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var officialContentExists = await dbContext.DailyMajlis.AnyAsync(
            dailyMajlis =>
                dailyMajlis.PublishDate == today &&
                (dailyMajlis.Status == DailyMajlisStatus.Scheduled ||
                 dailyMajlis.Status == DailyMajlisStatus.Published),
            cancellationToken);

        if (officialContentExists)
        {
            return;
        }

        var seedDailyMajlis = await dbContext.DailyMajlis
            .SingleOrDefaultAsync(
                dailyMajlis => dailyMajlis.Id == SeedDailyMajlisId,
                cancellationToken);

        if (seedDailyMajlis is not null)
        {
            dbContext.Entry(seedDailyMajlis)
                .Property(dailyMajlis => dailyMajlis.PublishDate)
                .CurrentValue = today;
            dbContext.Entry(seedDailyMajlis)
                .Property(dailyMajlis => dailyMajlis.Status)
                .CurrentValue = DailyMajlisStatus.Published;
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var seedChallenge = await dbContext.Challenges
            .Include(challenge => challenge.Options)
            .SingleOrDefaultAsync(
                challenge => challenge.Id == SeedChallengeId,
                cancellationToken);

        seedChallenge ??= CreateSeedChallenge();

        dbContext.DailyMajlis.Add(new DailyMajlisEntity(
            SeedDailyMajlisId,
            today,
            "The Guest Before the House",
            "hospitality",
            seedChallenge,
            "What is one hospitality habit your family still practices?",
            DailyMajlisStatus.Published));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Challenge CreateSeedChallenge()
    {
        return new Challenge(
            SeedChallengeId,
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
            ]);
    }
}
