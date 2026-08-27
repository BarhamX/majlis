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
    private static readonly Guid LegacyReplacementChallengeId =
        Guid.Parse("10000000-0000-0000-0000-000000000002");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var officialContentExists = await dbContext.DailyMajlis.AnyAsync(
            dailyMajlis =>
                dailyMajlis.PublishDate == today &&
                dailyMajlis.Status == DailyMajlisStatus.Published &&
                dailyMajlis.PublishedRevisionId != null,
            cancellationToken);
        if (officialContentExists)
        {
            return;
        }

        var seedDailyMajlis = await dbContext.DailyMajlis
            .SingleOrDefaultAsync(item => item.Id == SeedDailyMajlisId, cancellationToken);
        if (seedDailyMajlis is not null)
        {
            await CompleteLegacySeedAsync(seedDailyMajlis, today, cancellationToken);
            return;
        }

        var revisionId = Guid.NewGuid();
        var challenge = CreateSeedChallenge(revisionId);
        var revision = CreateSeedRevision(SeedDailyMajlisId, revisionId, challenge);
        dbContext.DailyMajlis.Add(new DailyMajlisEntity(
            SeedDailyMajlisId,
            today,
            DailyMajlisStatus.Published,
            revision));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CompleteLegacySeedAsync(
        DailyMajlisEntity seedDailyMajlis,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var revisionId = Guid.NewGuid();
        var challenge = CreateSeedChallenge(revisionId, LegacyReplacementChallengeId);
        var revisionNumber = (await dbContext.DailyMajlisRevisions
            .Where(revision => revision.DailyMajlisId == SeedDailyMajlisId)
            .Select(revision => (int?)revision.RevisionNumber)
            .MaxAsync(cancellationToken) ?? 0) + 1;
        var revision = CreateSeedRevision(SeedDailyMajlisId, revisionId, challenge, revisionNumber);

        dbContext.DailyMajlisRevisions.Add(revision);
        dbContext.Entry(seedDailyMajlis).Property(item => item.PublishDate).CurrentValue = today;
        dbContext.Entry(seedDailyMajlis).Property(item => item.Status).CurrentValue = DailyMajlisStatus.Published;
        dbContext.Entry(seedDailyMajlis).Property(item => item.PublishedRevisionId).CurrentValue = revision.Id;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private DailyMajlisRevision CreateSeedRevision(
        Guid dailyMajlisId,
        Guid revisionId,
        Challenge challenge,
        int revisionNumber = 1)
    {
        var revision = new DailyMajlisRevision(
            revisionId,
            dailyMajlisId,
            revisionNumber,
            "hospitality",
            ChallengeDifficulty.Easy,
            CardType.Proverb,
            "Seed content from the reviewed Development/Testing fixture.",
            createdByUserId: null,
            timeProvider.GetUtcNow());
        revision.SetChallenge(challenge);
        revision.AddTranslation(new DailyMajlisTranslation(
            revision.Id, "ar", "الضيف قبل البيت", "ما معنى هذا المثل؟",
            "يعكس المثل مكانة إكرام الضيف بوصفه شرفاً ومسؤولية.",
            "ما عادة الضيافة التي ما زالت أسرتك تحافظ عليها؟", "الضيف قبل البيت",
            cardTitle: "الضيف قبل البيت", cardMeaning: "إكرام الضيف شرف ومسؤولية."));
        revision.AddTranslation(new DailyMajlisTranslation(
            revision.Id, "en", "The Guest Before the House", "What does this proverb mean?",
            "This proverb reflects hospitality as honor and responsibility.",
            "What hospitality habit does your family still practice?", "The Guest Before the House",
            cardTitle: "The Guest Before the House",
            cardMeaning: "Honoring a guest is both an honor and a responsibility."));
        foreach (var option in challenge.Options)
        {
            revision.AddOptionTranslation(new ChallengeOptionTranslation(
                option.Id, "ar", option.IsCorrect ? "إكرام الضيف أمانة" : "لا ينبغي للضيف أن يطيل"));
            revision.AddOptionTranslation(new ChallengeOptionTranslation(
                option.Id, "en", option.IsCorrect ? "A guest is honored as a trust" : "A guest should not stay long"));
        }

        revision.AddRegion("gulf");
        revision.AddDialect("qa");
        return revision;
    }

    private static Challenge CreateSeedChallenge(Guid revisionId, Guid? challengeId = null)
    {
        return new Challenge(
            challengeId ?? SeedChallengeId,
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.NewGuid(), "a", true, 1),
                new ChallengeOption(Guid.NewGuid(), "b", false, 2),
            ]);
    }
}
