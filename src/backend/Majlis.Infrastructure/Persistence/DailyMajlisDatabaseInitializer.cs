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
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var repairableSeed = await GetRepairableSeedAsync(today, cancellationToken);
        if (IsUsablePublishedSeed(repairableSeed))
        {
            return;
        }

        if (await EditorialContentExistsAsync(today, cancellationToken))
        {
            return;
        }

        if (repairableSeed is not null)
        {
            await RepairSeedAsync(repairableSeed, today, cancellationToken);
            return;
        }

        await CreatePublishedSeedAsync(today, cancellationToken);
    }

    private async Task CreatePublishedSeedAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken)
    {
        var dailyMajlisId = Guid.NewGuid();
        var revisionId = Guid.NewGuid();
        var challenge = CreateSeedChallenge(revisionId);
        var revision = CreateSeedRevision(dailyMajlisId, revisionId, challenge);
        revision.Submit(timeProvider.GetUtcNow());
        var dailyMajlis = new DailyMajlisEntity(dailyMajlisId, publishDate);
        dailyMajlis.Publish(revision, timeProvider.GetUtcNow());
        dbContext.DailyMajlis.Add(dailyMajlis);

        try
        {
            await SaveNewPublicationAsync(dailyMajlis, revision, cancellationToken);
        }
        catch (DbUpdateException exception) when (
            DailyMajlisInitializationConflict.IsExpectedCreateRace(exception))
        {
            dbContext.ChangeTracker.Clear();
            if (!await OfficialContentExistsAsync(publishDate, cancellationToken))
            {
                throw;
            }
        }
    }

    private async Task RepairSeedAsync(
        DailyMajlisEntity seedDailyMajlis,
        DateOnly publishDate,
        CancellationToken cancellationToken)
    {
        var publicationWasMissing = seedDailyMajlis.Publication is null;
        try
        {
            await CompleteSeedAsync(seedDailyMajlis, cancellationToken);
        }
        catch (DbUpdateException exception) when (
            DailyMajlisInitializationConflict.IsExpectedRepairRace(
                exception,
                publicationWasMissing))
        {
            dbContext.ChangeTracker.Clear();
            var persistedSeed = await GetRepairableSeedAsync(publishDate, cancellationToken);
            if (!IsConvergedRepair(
                    persistedSeed,
                    seedDailyMajlis.Id,
                    publishDate))
            {
                throw;
            }
        }
    }

    private async Task CompleteSeedAsync(
        DailyMajlisEntity seedDailyMajlis,
        CancellationToken cancellationToken)
    {
        var revisionId = Guid.NewGuid();
        var challenge = CreateSeedChallenge(revisionId);
        var revisionNumber = (await dbContext.DailyMajlisRevisions
            .Where(revision => revision.DailyMajlisId == seedDailyMajlis.Id)
            .Select(revision => (int?)revision.RevisionNumber)
            .MaxAsync(cancellationToken) ?? 0) + 1;
        var revision = CreateSeedRevision(seedDailyMajlis.Id, revisionId, challenge, revisionNumber);
        revision.Submit(timeProvider.GetUtcNow());

        dbContext.DailyMajlisRevisions.Add(revision);
        seedDailyMajlis.Publish(revision, timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<DailyMajlisEntity?> GetRepairableSeedAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken) => dbContext.DailyMajlis
        .AsSplitQuery()
        .Include(item => item.Publication)
        .Include(item => item.PublishedRevision)
        .ThenInclude(revision => revision!.Translations)
        .Include(item => item.PublishedRevision)
        .ThenInclude(revision => revision!.Challenge)
        .ThenInclude(challenge => challenge!.Options)
        .ThenInclude(option => option.Translations)
        .Where(item =>
            item.PublishDate == publishDate &&
            (item.Id == SeedDailyMajlisId || item.Publication != null))
        .OrderByDescending(item => item.Publication != null)
        .ThenByDescending(item => item.Id == SeedDailyMajlisId)
        .FirstOrDefaultAsync(cancellationToken);

    private static bool IsUsablePublishedSeed(DailyMajlisEntity? seedDailyMajlis) =>
        seedDailyMajlis is
        {
            Status: DailyMajlisStatus.Published,
            PublishedRevision.IsImmutable: true,
        } && seedDailyMajlis.PublishedRevision.IsCompleteForServing();

    private static bool IsConvergedRepair(
        DailyMajlisEntity? persistedSeed,
        Guid expectedDailyMajlisId,
        DateOnly expectedPublishDate) =>
        IsUsablePublishedSeed(persistedSeed) &&
        persistedSeed!.Id == expectedDailyMajlisId &&
        persistedSeed.Publication is not null &&
        persistedSeed.Publication.DailyMajlisId == expectedDailyMajlisId &&
        persistedSeed.Publication.PublishDate == expectedPublishDate;

    private Task<bool> EditorialContentExistsAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken) => dbContext.DailyMajlis.AnyAsync(
        dailyMajlis =>
            dailyMajlis.PublishDate == publishDate &&
            (dailyMajlis.Status == DailyMajlisStatus.Scheduled ||
             (dailyMajlis.Status == DailyMajlisStatus.Published &&
              dailyMajlis.Id != SeedDailyMajlisId)),
        cancellationToken);

    private async Task SaveNewPublicationAsync(
        DailyMajlisEntity dailyMajlis,
        DailyMajlisRevision revision,
        CancellationToken cancellationToken)
    {
        var publishedRevision = dbContext.Entry(dailyMajlis)
            .Reference(item => item.PublishedRevision);
        publishedRevision.CurrentValue = null;
        dbContext.Entry(dailyMajlis)
            .Property(item => item.PublishedRevisionId)
            .CurrentValue = null;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        dailyMajlis.Publish(revision, timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private Task<bool> OfficialContentExistsAsync(
        DateOnly publishDate,
        CancellationToken cancellationToken) => dbContext.DailyMajlis.AnyAsync(
        dailyMajlis =>
            dailyMajlis.PublishDate == publishDate &&
            (dailyMajlis.Status == DailyMajlisStatus.Scheduled ||
             dailyMajlis.Status == DailyMajlisStatus.Published),
        cancellationToken);

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

    private static Challenge CreateSeedChallenge(Guid revisionId)
    {
        return new Challenge(
            Guid.NewGuid(),
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.NewGuid(), "a", true, 1),
                new ChallengeOption(Guid.NewGuid(), "b", false, 2),
            ]);
    }
}
