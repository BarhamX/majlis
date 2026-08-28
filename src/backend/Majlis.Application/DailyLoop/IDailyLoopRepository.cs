using Majlis.Application.Identity;
using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Domain.Progress;
using DailyMajlisEntity = Majlis.Domain.DailyMajlis.DailyMajlis;

namespace Majlis.Application.DailyLoop;

public interface IDailyLoopRepository
{
    Task<T> ExecuteInTransactionAsync<T>(
        Func<IDailyLoopTransaction, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken);

    Task<Guid?> ResolveUserIdAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken);

    Task<StoredAttemptResult?> GetAttemptResultAsync(
        Guid userId,
        Guid attemptId,
        CancellationToken cancellationToken);

    Task<Guid?> GetAttemptIdAsync(
        Guid userId,
        Guid dailyMajlisId,
        CancellationToken cancellationToken);

    Task<UserProgress?> GetProgressAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StoredAttemptHistoryItem>> GetAttemptHistoryAsync(
        Guid userId,
        AttemptHistoryBoundary? boundary,
        int take,
        CancellationToken cancellationToken);

    Task<SubmissionConflictState> ResolveSubmissionConflictAsync(
        AuthenticatedIdentity identity,
        Guid challengeId,
        string scope,
        Guid key,
        CancellationToken cancellationToken);
}

public interface IDailyLoopTransaction
{
    Task<UserAccount?> LockUserAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken);

    Task<IdempotencyRecord?> FindIdempotencyAsync(
        Guid userId,
        string scope,
        Guid key,
        CancellationToken cancellationToken);

    Task<Guid?> FindDailyMajlisIdForChallengeAsync(
        Guid challengeId,
        CancellationToken cancellationToken);

    Task<UserAttempt?> FindAttemptAsync(
        Guid userId,
        Guid dailyMajlisId,
        CancellationToken cancellationToken);

    Task<DailyMajlisEntity?> GetCurrentPublishedChallengeAsync(
        DateOnly publishDate,
        Guid challengeId,
        CancellationToken cancellationToken);

    Task<UserProgress?> GetProgressForUpdateAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DateOnly>> GetPublishedDatesAsync(
        DateOnly? after,
        DateOnly through,
        CancellationToken cancellationToken);

    void AddAttempt(UserAttempt attempt);

    void AddLedgerEntry(XpLedgerEntry entry);

    void AddProgress(UserProgress progress);

    void AddIdempotencyRecord(IdempotencyRecord record);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record StoredAttemptResult(
    UserAttempt Attempt,
    DailyMajlisEntity DailyMajlis,
    DailyMajlisRevision Revision);

public sealed record StoredAttemptHistoryItem(
    UserAttempt Attempt,
    DateOnly PublishDate,
    string Title);

public sealed record AttemptHistoryBoundary(
    DateTimeOffset AttemptedAt,
    Guid AttemptId);

public sealed record SubmissionConflictState(
    Guid? UserId,
    IdempotencyRecord? IdempotencyRecord,
    UserAttempt? ExistingAttempt);
