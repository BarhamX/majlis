using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Majlis.Application.DailyMajlis;
using Majlis.Application.Identity;
using Majlis.Contracts.DailyLoop;
using Majlis.Contracts.DailyMajlis;
using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Domain.Progress;

namespace Majlis.Application.DailyLoop;

public sealed class DailyLoopService(
    IDailyLoopRepository repository,
    TimeProvider timeProvider,
    ShareLinkSettings shareLinkSettings) : IDailyLoopService
{
    private const string AttemptScope = "challenge_attempt";

    public async Task<AttemptSubmissionResult> SubmitAttemptAsync(
        AuthenticatedIdentity identity,
        Guid challengeId,
        Guid selectedOptionId,
        Guid idempotencyKey,
        string? acceptLanguage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RequireId(challengeId, nameof(challengeId));
        RequireId(selectedOptionId, nameof(selectedOptionId));
        RequireId(idempotencyKey, nameof(idempotencyKey));

        var requestHash = CreateRequestHash(challengeId, selectedOptionId);
        SubmissionDecision decision;
        try
        {
            decision = await repository.ExecuteInTransactionAsync(
                async (transaction, token) =>
                {
                    var user = await transaction.LockUserAsync(identity, token);
                    var now = timeProvider.GetUtcNow();
                    var today = DateOnly.FromDateTime(now.UtcDateTime);
                    ValidateEligibleUser(user, identity);

                    var idempotency = await transaction.FindIdempotencyAsync(
                        user!.Id,
                        AttemptScope,
                        idempotencyKey,
                        token);
                    if (idempotency is not null)
                    {
                        if (!string.Equals(
                                idempotency.RequestHash,
                                requestHash,
                                StringComparison.Ordinal))
                        {
                            throw ReusedIdempotencyKey();
                        }

                        return new SubmissionDecision(
                            user.Id,
                            idempotency.ResourceId ?? throw new InvalidOperationException(
                                "An accepted attempt idempotency record must identify its attempt."),
                            IsReplay: true);
                    }

                    var dailyMajlisId = await transaction.FindDailyMajlisIdForChallengeAsync(
                        challengeId,
                        token);
                    if (dailyMajlisId.HasValue)
                    {
                        var existingAttempt = await transaction.FindAttemptAsync(
                            user.Id,
                            dailyMajlisId.Value,
                            token);
                        if (existingAttempt is not null)
                        {
                            throw AttemptCompleted(existingAttempt.Id);
                        }
                    }

                    var dailyMajlis = await transaction.GetCurrentPublishedChallengeAsync(
                        today,
                        challengeId,
                        token);
                    var revision = dailyMajlis?.PublishedRevision;
                    var challenge = revision?.Challenge;
                    if (dailyMajlis is null || revision is null || challenge is null ||
                        !revision.IsImmutable || !revision.IsCompleteForServing())
                    {
                        throw new DailyLoopException(
                            "daily_majlis_unavailable",
                            "The requested challenge is not available for submission today.");
                    }

                    var selectedOption = challenge.Options.SingleOrDefault(
                        option => option.Id == selectedOptionId);
                    if (selectedOption is null)
                    {
                        throw new DailyLoopException(
                            "option_not_in_challenge",
                            "The selected option does not belong to this challenge.");
                    }

                    var resultLocale = LocaleSelector.Select(acceptLanguage, revision);
                    var score = AttemptScoring.Calculate(selectedOption.IsCorrect);
                    var progress = await transaction.GetProgressForUpdateAsync(user.Id, token);
                    if (progress is null)
                    {
                        progress = new UserProgress(user.Id, now);
                        transaction.AddProgress(progress);
                    }

                    var publishedDates = await transaction.GetPublishedDatesAsync(
                        progress.LastCompletedPublishDate,
                        today,
                        token);
                    progress.ApplyAttempt(score, dailyMajlis.PublishDate, publishedDates, now);

                    var attemptId = Guid.NewGuid();
                    var attempt = new UserAttempt(
                        attemptId,
                        user.Id,
                        dailyMajlis.Id,
                        challenge.Id,
                        revision.Id,
                        selectedOption.Id,
                        selectedOption.IsCorrect,
                        score,
                        resultLocale,
                        progress.LifetimeXp,
                        progress.CurrentStreak,
                        progress.LongestStreak,
                        now);
                    transaction.AddAttempt(attempt);
                    transaction.AddLedgerEntry(new XpLedgerEntry(
                        Guid.NewGuid(),
                        user.Id,
                        attemptId,
                        score.TotalXp,
                        now));
                    transaction.AddIdempotencyRecord(new IdempotencyRecord(
                        user.Id,
                        AttemptScope,
                        idempotencyKey,
                        requestHash,
                        attemptId,
                        responseStatus: 201,
                        now,
                        now.AddHours(24)));
                    await transaction.SaveChangesAsync(token);

                    return new SubmissionDecision(user.Id, attemptId, IsReplay: false);
                },
                cancellationToken);
        }
        catch (DailyLoopPersistenceConflictException)
        {
            decision = await ResolveConcurrencyAsync(
                identity,
                challengeId,
                idempotencyKey,
                requestHash,
                cancellationToken);
        }

        var stored = await repository.GetAttemptResultAsync(
            decision.UserId,
            decision.AttemptId,
            cancellationToken) ?? throw new InvalidOperationException(
                "The accepted attempt could not be read after its transaction completed.");
        return new AttemptSubmissionResult(MapResult(stored), decision.IsReplay);
    }

    public async Task<AttemptResultResponse?> GetAttemptAsync(
        AuthenticatedIdentity identity,
        Guid attemptId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RequireId(attemptId, nameof(attemptId));
        var userId = await repository.ResolveUserIdAsync(identity, cancellationToken);
        if (!userId.HasValue)
        {
            return null;
        }

        var stored = await repository.GetAttemptResultAsync(
            userId.Value,
            attemptId,
            cancellationToken);
        return stored is null ? null : MapResult(stored);
    }

    public async Task<AttemptHistoryResponse> GetAttemptHistoryAsync(
        AuthenticatedIdentity identity,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (limit is < 1 or > 50)
        {
            throw new DailyLoopException(
                "validation_failed",
                "Attempt history limit must be between 1 and 50.");
        }

        var boundary = string.IsNullOrWhiteSpace(cursor)
            ? null
            : AttemptCursorCodec.Decode(cursor);
        var userId = await repository.ResolveUserIdAsync(identity, cancellationToken);
        if (!userId.HasValue)
        {
            return new AttemptHistoryResponse([], null);
        }

        var rows = await repository.GetAttemptHistoryAsync(
            userId.Value,
            boundary,
            limit + 1,
            cancellationToken);
        var hasMore = rows.Count > limit;
        var page = rows.Take(limit).ToArray();
        var items = page.Select(row => new AttemptHistoryItemResponse(
            row.Attempt.Id,
            row.PublishDate,
            row.Title,
            row.Attempt.IsCorrect,
            row.Attempt.CompletionXp + row.Attempt.CorrectnessXp,
            row.Attempt.ResultLocale,
            row.Attempt.ContentRevisionId)).ToArray();
        var nextCursor = hasMore && page.Length > 0
            ? AttemptCursorCodec.Encode(new AttemptHistoryBoundary(
                page[^1].Attempt.AttemptedAt,
                page[^1].Attempt.Id))
            : null;
        return new AttemptHistoryResponse(items, nextCursor);
    }

    public async Task<UserProgressResponse> GetProgressAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        var userId = await repository.ResolveUserIdAsync(identity, cancellationToken);
        if (!userId.HasValue)
        {
            return EmptyProgress();
        }

        var progress = await repository.GetProgressAsync(userId.Value, cancellationToken);
        return progress is null
            ? EmptyProgress()
            : new UserProgressResponse(
                progress.LifetimeXp,
                progress.CurrentStreak,
                progress.LongestStreak,
                progress.LastCompletedPublishDate);
    }

    public async Task<AttemptShareResponse?> GetShareAsync(
        AuthenticatedIdentity identity,
        Guid attemptId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetStoredAttemptAsync(identity, attemptId, cancellationToken);
        if (result is null)
        {
            return null;
        }

        var publishDate = result.DailyMajlis.PublishDate;
        return new AttemptShareResponse(
            publishDate,
            "completed",
            "أكملت مجلس اليوم",
            "هل تعرف الإجابة؟",
            $"{shareLinkSettings.PublicHost}/daily/{publishDate:yyyy-MM-dd}",
            "بطاقة مجلس خالية من حرق الإجابة");
    }

    public async Task<DailyMajlisUserStateResponse> GetTodayStateAsync(
        AuthenticatedIdentity identity,
        Guid dailyMajlisId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RequireId(dailyMajlisId, nameof(dailyMajlisId));
        var userId = await repository.ResolveUserIdAsync(identity, cancellationToken);
        if (!userId.HasValue)
        {
            return new DailyMajlisUserStateResponse(false, null);
        }

        var attemptId = await repository.GetAttemptIdAsync(
            userId.Value,
            dailyMajlisId,
            cancellationToken);
        return new DailyMajlisUserStateResponse(attemptId.HasValue, attemptId);
    }

    private async Task<StoredAttemptResult?> GetStoredAttemptAsync(
        AuthenticatedIdentity identity,
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RequireId(attemptId, nameof(attemptId));
        var userId = await repository.ResolveUserIdAsync(identity, cancellationToken);
        return userId.HasValue
            ? await repository.GetAttemptResultAsync(userId.Value, attemptId, cancellationToken)
            : null;
    }

    private async Task<SubmissionDecision> ResolveConcurrencyAsync(
        AuthenticatedIdentity identity,
        Guid challengeId,
        Guid idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var conflict = await repository.ResolveSubmissionConflictAsync(
            identity,
            challengeId,
            AttemptScope,
            idempotencyKey,
            cancellationToken);
        if (conflict.IdempotencyRecord is not null)
        {
            if (!string.Equals(
                    conflict.IdempotencyRecord.RequestHash,
                    requestHash,
                    StringComparison.Ordinal))
            {
                throw ReusedIdempotencyKey();
            }

            return new SubmissionDecision(
                conflict.UserId ?? throw new InvalidOperationException(
                    "A persisted idempotency conflict must identify its user."),
                conflict.IdempotencyRecord.ResourceId ?? throw new InvalidOperationException(
                    "A persisted attempt idempotency record must identify its attempt."),
                IsReplay: true);
        }

        if (conflict.ExistingAttempt is not null)
        {
            throw AttemptCompleted(conflict.ExistingAttempt.Id);
        }

        throw new InvalidOperationException(
            "A daily-loop persistence conflict did not converge on a persisted result.");
    }

    private static AttemptResultResponse MapResult(StoredAttemptResult stored)
    {
        var attempt = stored.Attempt;
        var revision = stored.Revision;
        var translation = revision.Translations.SingleOrDefault(item =>
            string.Equals(item.Locale, attempt.ResultLocale, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                "The stored result locale is missing from the immutable content revision.");
        var challenge = revision.Challenge ?? throw new InvalidOperationException(
            "The stored content revision has no challenge.");
        var correctOptionId = challenge.Options.Single(option => option.IsCorrect).Id;

        return new AttemptResultResponse(
            attempt.Id,
            attempt.DailyMajlisId,
            stored.DailyMajlis.PublishDate,
            attempt.IsCorrect,
            correctOptionId,
            translation.Explanation,
            new CulturalCardResponse(
                MapCardType(revision.CardType),
                translation.CardTitle,
                translation.CardText,
                translation.CardMeaning,
                translation.CardContext,
                translation.PublicAttribution),
            new AttemptXpResponse(
                attempt.CompletionXp,
                attempt.CorrectnessXp,
                attempt.CompletionXp + attempt.CorrectnessXp,
                attempt.LifetimeXpAfter),
            new AttemptStreakResponse(
                attempt.CurrentStreakAfter,
                attempt.LongestStreakAfter,
                Updated: true),
            attempt.ContentRevisionId,
            attempt.ResultLocale);
    }

    private static void ValidateEligibleUser(
        UserAccount? user,
        AuthenticatedIdentity identity)
    {
        if (user is null || user.Status != UserAccountStatus.Active)
        {
            throw new DailyLoopException(
                "forbidden",
                "The authenticated account is not active.");
        }

        if (user.Profile is null)
        {
            throw new DailyLoopException(
                "profile_incomplete",
                "Complete the Majlis profile before submitting an attempt.");
        }

        if (user.AuthenticationNotBefore.HasValue &&
            identity.IssuedAt <= user.AuthenticationNotBefore.Value)
        {
            throw new DailyLoopException(
                "forbidden",
                "The authenticated credential is no longer valid.");
        }
    }

    private static string CreateRequestHash(Guid challengeId, Guid selectedOptionId)
    {
        var canonical = string.Create(
            CultureInfo.InvariantCulture,
            $"challenge:{challengeId:D}\nselectedOption:{selectedOptionId:D}");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static string MapCardType(CardType type) => type switch
    {
        CardType.Proverb => "proverb",
        CardType.Story => "story",
        CardType.Saying => "saying",
        CardType.Tradition => "tradition",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown card type."),
    };

    private static UserProgressResponse EmptyProgress() => new(0, 0, 0, null);

    private static DailyLoopException ReusedIdempotencyKey() => new(
        "idempotency_key_reused",
        "The idempotency key was already used with another request payload.");

    private static DailyLoopException AttemptCompleted(Guid attemptId) => new(
        "attempt_already_completed",
        "This Daily Majlis attempt is already completed.",
        attemptId);

    private static Guid RequireId(Guid id, string parameterName) => id == Guid.Empty
        ? throw new ArgumentException("A non-empty id is required.", parameterName)
        : id;

    private sealed record SubmissionDecision(Guid UserId, Guid AttemptId, bool IsReplay);
}

internal static class AttemptCursorCodec
{
    public static string Encode(AttemptHistoryBoundary boundary)
    {
        var value = string.Create(
            CultureInfo.InvariantCulture,
            $"{boundary.AttemptedAt.UtcDateTime.Ticks}:{boundary.AttemptId:N}");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static AttemptHistoryBoundary Decode(string value)
    {
        try
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded += new string('=', (4 - padded.Length % 4) % 4);
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            var parts = decoded.Split(':', 2);
            if (parts.Length != 2 ||
                !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks) ||
                !Guid.TryParseExact(parts[1], "N", out var attemptId))
            {
                throw InvalidCursor();
            }

            return new AttemptHistoryBoundary(
                new DateTimeOffset(ticks, TimeSpan.Zero),
                attemptId);
        }
        catch (Exception exception) when (
            exception is FormatException or ArgumentOutOfRangeException)
        {
            throw InvalidCursor();
        }
    }

    private static DailyLoopException InvalidCursor() => new(
        "validation_failed",
        "The attempt history cursor is invalid.");
}
