using Majlis.Application.Identity;
using Majlis.Contracts.DailyLoop;
using Majlis.Contracts.DailyMajlis;

namespace Majlis.Application.DailyLoop;

public interface IDailyLoopService
{
    Task<AttemptSubmissionResult> SubmitAttemptAsync(
        AuthenticatedIdentity identity,
        Guid challengeId,
        Guid selectedOptionId,
        Guid idempotencyKey,
        string? acceptLanguage,
        CancellationToken cancellationToken = default);

    Task<AttemptResultResponse?> GetAttemptAsync(
        AuthenticatedIdentity identity,
        Guid attemptId,
        CancellationToken cancellationToken = default);

    Task<AttemptHistoryResponse> GetAttemptHistoryAsync(
        AuthenticatedIdentity identity,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default);

    Task<UserProgressResponse> GetProgressAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken = default);

    Task<AttemptShareResponse?> GetShareAsync(
        AuthenticatedIdentity identity,
        Guid attemptId,
        CancellationToken cancellationToken = default);

    Task<DailyMajlisUserStateResponse> GetTodayStateAsync(
        AuthenticatedIdentity identity,
        Guid dailyMajlisId,
        CancellationToken cancellationToken = default);
}

public sealed record AttemptSubmissionResult(
    AttemptResultResponse Response,
    bool IsReplay);
