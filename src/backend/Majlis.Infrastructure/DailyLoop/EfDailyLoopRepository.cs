using Majlis.Application.DailyLoop;
using Majlis.Application.Identity;
using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Infrastructure.DailyLoop;

internal sealed class EfDailyLoopRepository(MajlisDbContext dbContext) :
    IDailyLoopRepository,
    IDailyLoopTransaction
{
    private const int MaximumTransactionAttempts = 3;

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<IDailyLoopTransaction, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        for (var attempt = 1; attempt <= MaximumTransactionAttempts; attempt++)
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                cancellationToken);
            try
            {
                var result = await operation(this, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception exception) when (IsConcurrencyConflict(exception))
            {
                await transaction.RollbackAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
                if (attempt == MaximumTransactionAttempts)
                {
                    throw new DailyLoopPersistenceConflictException(exception);
                }
            }
        }

        throw new InvalidOperationException("The daily-loop transaction retry loop did not complete.");
    }

    public async Task<UserAccount?> LockUserAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken)
    {
        var userId = await ResolveUserIdAsync(identity, cancellationToken);
        if (!userId.HasValue)
        {
            return null;
        }

        var user = await dbContext.Users
            .FromSqlInterpolated($$"""
                SELECT *
                FROM "Users"
                WHERE "Id" = {{userId.Value}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);
        if (user is not null)
        {
            await dbContext.Entry(user)
                .Reference(item => item.Profile)
                .LoadAsync(cancellationToken);
        }

        return user;
    }

    public Task<IdempotencyRecord?> FindIdempotencyAsync(
        Guid userId,
        string scope,
        Guid key,
        CancellationToken cancellationToken) => dbContext.IdempotencyRecords
        .SingleOrDefaultAsync(record =>
            record.UserId == userId &&
            record.Scope == scope &&
            record.IdempotencyKey == key,
            cancellationToken);

    public async Task<Guid?> FindDailyMajlisIdForChallengeAsync(
        Guid challengeId,
        CancellationToken cancellationToken) => await (
            from challenge in dbContext.Challenges
            join revision in dbContext.DailyMajlisRevisions
                on challenge.RevisionId equals revision.Id
            where challenge.Id == challengeId
            select (Guid?)revision.DailyMajlisId)
        .SingleOrDefaultAsync(cancellationToken);

    public Task<UserAttempt?> FindAttemptAsync(
        Guid userId,
        Guid dailyMajlisId,
        CancellationToken cancellationToken) => dbContext.UserAttempts
        .SingleOrDefaultAsync(attempt =>
            attempt.UserId == userId && attempt.DailyMajlisId == dailyMajlisId,
            cancellationToken);

    public async Task<DailyMajlisEntity?> GetCurrentPublishedChallengeAsync(
        DateOnly publishDate,
        Guid challengeId,
        CancellationToken cancellationToken)
    {
        var dailyMajlis = await dbContext.DailyMajlis
            .FromSqlInterpolated($$"""
                SELECT daily.*
                FROM "DailyMajlis" AS daily
                INNER JOIN "DailyMajlisRevisions" AS revision
                    ON revision."Id" = daily."PublishedRevisionId"
                INNER JOIN "Challenges" AS challenge
                    ON challenge."RevisionId" = revision."Id"
                WHERE daily."PublishDate" = {{publishDate}}
                  AND daily."Status" = 'published'
                  AND challenge."Id" = {{challengeId}}
                FOR SHARE OF daily
                """)
            .SingleOrDefaultAsync(cancellationToken);
        if (dailyMajlis is null)
        {
            return null;
        }

        await dbContext.Entry(dailyMajlis)
            .Reference(item => item.PublishedRevision)
            .LoadAsync(cancellationToken);
        var revision = dailyMajlis.PublishedRevision!;
        await dbContext.Entry(revision).Collection(item => item.Translations)
            .LoadAsync(cancellationToken);
        await dbContext.Entry(revision).Reference(item => item.Challenge)
            .LoadAsync(cancellationToken);
        var challenge = revision.Challenge!;
        await dbContext.Entry(challenge).Collection(item => item.Options)
            .LoadAsync(cancellationToken);
        foreach (var option in challenge.Options)
        {
            await dbContext.Entry(option).Collection(item => item.Translations)
                .LoadAsync(cancellationToken);
        }

        return dailyMajlis;
    }

    public Task<UserProgress?> GetProgressForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken) => dbContext.UserProgress
        .SingleOrDefaultAsync(progress => progress.UserId == userId, cancellationToken);

    public async Task<IReadOnlyCollection<DateOnly>> GetPublishedDatesAsync(
        DateOnly? after,
        DateOnly through,
        CancellationToken cancellationToken) => await dbContext.DailyMajlisPublications
        .AsNoTracking()
        .Where(publication =>
            (!after.HasValue || publication.PublishDate >= after.Value) &&
            publication.PublishDate <= through)
        .OrderBy(publication => publication.PublishDate)
        .Select(publication => publication.PublishDate)
        .ToArrayAsync(cancellationToken);

    public void AddAttempt(UserAttempt attempt) => dbContext.UserAttempts.Add(attempt);

    public void AddLedgerEntry(XpLedgerEntry entry) => dbContext.XpLedger.Add(entry);

    public void AddProgress(UserProgress progress) => dbContext.UserProgress.Add(progress);

    public void AddIdempotencyRecord(IdempotencyRecord record) =>
        dbContext.IdempotencyRecords.Add(record);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<Guid?> ResolveUserIdAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken) => await dbContext.UserIdentities
        .AsNoTracking()
        .Where(item =>
            item.Provider == identity.Provider &&
            item.Issuer == identity.Issuer &&
            item.Subject == identity.Subject)
        .Select(item => (Guid?)item.UserId)
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<StoredAttemptResult?> GetAttemptResultAsync(
        Guid userId,
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var attempt = await dbContext.UserAttempts
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.UserId == userId && item.Id == attemptId,
                cancellationToken);
        if (attempt is null)
        {
            return null;
        }

        var dailyMajlis = await dbContext.DailyMajlis
            .AsNoTracking()
            .SingleAsync(item => item.Id == attempt.DailyMajlisId, cancellationToken);
        var revision = await dbContext.DailyMajlisRevisions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(item => item.Translations)
            .Include(item => item.Challenge)
            .ThenInclude(challenge => challenge!.Options)
            .SingleAsync(item => item.Id == attempt.ContentRevisionId, cancellationToken);
        return new StoredAttemptResult(attempt, dailyMajlis, revision);
    }

    public async Task<Guid?> GetAttemptIdAsync(
        Guid userId,
        Guid dailyMajlisId,
        CancellationToken cancellationToken) => await dbContext.UserAttempts
        .AsNoTracking()
        .Where(item => item.UserId == userId && item.DailyMajlisId == dailyMajlisId)
        .Select(item => (Guid?)item.Id)
        .SingleOrDefaultAsync(cancellationToken);

    public Task<UserProgress?> GetProgressAsync(
        Guid userId,
        CancellationToken cancellationToken) => dbContext.UserProgress
        .AsNoTracking()
        .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<StoredAttemptHistoryItem>> GetAttemptHistoryAsync(
        Guid userId,
        AttemptHistoryBoundary? boundary,
        int take,
        CancellationToken cancellationToken)
    {
        var attempts = boundary is null
            ? await dbContext.UserAttempts
                .FromSqlInterpolated($$"""
                    SELECT *
                    FROM "UserAttempts"
                    WHERE "UserId" = {{userId}}
                    ORDER BY "AttemptedAt" DESC, "Id" DESC
                    LIMIT {{take}}
                    """)
                .AsNoTracking()
                .ToArrayAsync(cancellationToken)
            : await dbContext.UserAttempts
                .FromSqlInterpolated($$"""
                    SELECT *
                    FROM "UserAttempts"
                    WHERE "UserId" = {{userId}}
                      AND (
                        "AttemptedAt" < {{boundary.AttemptedAt}}
                        OR ("AttemptedAt" = {{boundary.AttemptedAt}} AND "Id" < {{boundary.AttemptId}})
                      )
                    ORDER BY "AttemptedAt" DESC, "Id" DESC
                    LIMIT {{take}}
                    """)
                .AsNoTracking()
                .ToArrayAsync(cancellationToken);
        if (attempts.Length == 0)
        {
            return [];
        }

        var dailyMajlisIds = attempts.Select(item => item.DailyMajlisId).Distinct().ToArray();
        var publishDates = await dbContext.DailyMajlis
            .AsNoTracking()
            .Where(item => dailyMajlisIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.PublishDate, cancellationToken);
        var revisionIds = attempts.Select(item => item.ContentRevisionId).Distinct().ToArray();
        var revisions = await dbContext.DailyMajlisRevisions
            .AsNoTracking()
            .Where(item => revisionIds.Contains(item.Id))
            .Include(item => item.Translations)
            .ToDictionaryAsync(item => item.Id, cancellationToken);

        return attempts.Select(attempt =>
        {
            var revision = revisions[attempt.ContentRevisionId];
            var title = revision.Translations.Single(item => string.Equals(
                item.Locale,
                attempt.ResultLocale,
                StringComparison.OrdinalIgnoreCase)).Title;
            return new StoredAttemptHistoryItem(
                attempt,
                publishDates[attempt.DailyMajlisId],
                title);
        }).ToArray();
    }

    public async Task<SubmissionConflictState> ResolveSubmissionConflictAsync(
        AuthenticatedIdentity identity,
        Guid challengeId,
        string scope,
        Guid key,
        CancellationToken cancellationToken)
    {
        dbContext.ChangeTracker.Clear();
        var userId = await ResolveUserIdAsync(identity, cancellationToken);
        if (!userId.HasValue)
        {
            return new SubmissionConflictState(null, null, null);
        }

        var idempotency = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(record =>
                record.UserId == userId.Value &&
                record.Scope == scope &&
                record.IdempotencyKey == key,
                cancellationToken);
        var dailyMajlisId = await FindDailyMajlisIdForChallengeAsync(
            challengeId,
            cancellationToken);
        var existingAttempt = dailyMajlisId.HasValue
            ? await dbContext.UserAttempts
                .AsNoTracking()
                .SingleOrDefaultAsync(attempt =>
                    attempt.UserId == userId.Value &&
                    attempt.DailyMajlisId == dailyMajlisId.Value,
                    cancellationToken)
            : null;
        return new SubmissionConflictState(userId, idempotency, existingAttempt);
    }

    private static bool IsConcurrencyConflict(Exception exception) => exception switch
    {
        DbUpdateConcurrencyException => true,
        DbUpdateException
        {
            InnerException: PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation or
                    PostgresErrorCodes.SerializationFailure or
                    PostgresErrorCodes.DeadlockDetected,
            },
        } => true,
        PostgresException
        {
            SqlState: PostgresErrorCodes.SerializationFailure or
                PostgresErrorCodes.DeadlockDetected,
        } => true,
        _ => false,
    };
}
