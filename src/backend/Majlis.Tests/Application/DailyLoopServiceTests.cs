using Majlis.Application.DailyLoop;
using Majlis.Application.Identity;
using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Domain.Progress;

namespace Majlis.Tests.Application;

public sealed class DailyLoopServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly AuthenticatedIdentity Identity = new(
        ExternalIdentityProvider.Test,
        "https://test.majlis.local",
        "daily-loop-user",
        Now.AddMinutes(-1));

    [Theory]
    [InlineData(false, 10, 0, 10)]
    [InlineData(true, 10, 5, 15)]
    public async Task SubmitAttempt_FirstAcceptedAttempt_PersistsExactAwardAndSnapshots(
        bool selectCorrectOption,
        int expectedCompletionXp,
        int expectedCorrectnessXp,
        int expectedTotalXp)
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);
        var selectedOption = selectCorrectOption
            ? repository.CorrectOption
            : repository.IncorrectOption;

        var result = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            selectedOption.Id,
            Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "en-US");

        Assert.False(result.IsReplay);
        Assert.Equal(selectCorrectOption, result.Response.IsCorrect);
        Assert.Equal(expectedCompletionXp, result.Response.Xp.Completion);
        Assert.Equal(expectedCorrectnessXp, result.Response.Xp.Correctness);
        Assert.Equal(expectedTotalXp, result.Response.Xp.Awarded);
        Assert.Equal(expectedTotalXp, result.Response.Xp.LifetimeTotal);
        Assert.Equal(1, result.Response.Streak.Current);
        Assert.Equal(1, result.Response.Streak.Longest);
        Assert.Equal("en", result.Response.ResultLocale);
        Assert.Equal("English explanation", result.Response.Explanation);

        var attempt = Assert.Single(repository.Attempts);
        Assert.Equal(result.Response.AttemptId, attempt.Id);
        Assert.Equal(expectedTotalXp, attempt.LifetimeXpAfter);
        Assert.Equal("en", attempt.ResultLocale);
        Assert.Equal(expectedTotalXp, Assert.Single(repository.Ledger).Amount);
        Assert.Equal(expectedTotalXp, repository.Progress!.LifetimeXp);
        Assert.Single(repository.IdempotencyRecords);
    }

    [Fact]
    public async Task SubmitAttempt_SameKeyAndPayload_ReturnsStoredReplayWithoutMutation()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);
        var key = Guid.Parse("10000000-0000-0000-0000-000000000002");

        var first = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.CorrectOption.Id,
            key,
            "en");
        var replay = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.CorrectOption.Id,
            key,
            "ar");

        Assert.True(replay.IsReplay);
        Assert.Equal(first.Response, replay.Response);
        Assert.Single(repository.Attempts);
        Assert.Single(repository.Ledger);
        Assert.Single(repository.IdempotencyRecords);
        Assert.Equal(15, repository.Progress!.LifetimeXp);
    }

    [Fact]
    public async Task SubmitAttempt_SameKeyWithChangedOption_RejectsReusedKey()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);
        var key = Guid.Parse("10000000-0000-0000-0000-000000000003");
        await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.CorrectOption.Id,
            key,
            "ar");

        var exception = await Assert.ThrowsAsync<DailyLoopException>(() =>
            service.SubmitAttemptAsync(
                Identity,
                repository.Challenge.Id,
                repository.IncorrectOption.Id,
                key,
                "ar"));

        Assert.Equal("idempotency_key_reused", exception.Code);
        Assert.Single(repository.Attempts);
        Assert.Equal(15, repository.Progress!.LifetimeXp);
    }

    [Fact]
    public async Task SubmitAttempt_DifferentKeyAfterCompletion_ReturnsExistingAttemptConflict()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);
        var first = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.IncorrectOption.Id,
            Guid.Parse("10000000-0000-0000-0000-000000000004"),
            "ar");

        var exception = await Assert.ThrowsAsync<DailyLoopException>(() =>
            service.SubmitAttemptAsync(
                Identity,
                repository.Challenge.Id,
                repository.IncorrectOption.Id,
                Guid.Parse("10000000-0000-0000-0000-000000000005"),
                "ar"));

        Assert.Equal("attempt_already_completed", exception.Code);
        Assert.Equal(first.Response.AttemptId, exception.AttemptId);
        Assert.Single(repository.Attempts);
        Assert.Equal(10, repository.Progress!.LifetimeXp);
    }

    [Fact]
    public async Task SubmitAttempt_WhenChallengeIsNotTodaysPublishedChallenge_RejectsWithoutMutation()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<DailyLoopException>(() =>
            service.SubmitAttemptAsync(
                Identity,
                Guid.Parse("20000000-0000-0000-0000-000000000001"),
                repository.CorrectOption.Id,
                Guid.Parse("10000000-0000-0000-0000-000000000006"),
                "ar"));

        Assert.Equal("daily_majlis_unavailable", exception.Code);
        Assert.Empty(repository.Attempts);
        Assert.Empty(repository.Ledger);
        Assert.Empty(repository.IdempotencyRecords);
        Assert.Null(repository.Progress);
    }

    [Fact]
    public async Task SubmitAttempt_WhenOptionBelongsToAnotherChallenge_RejectsWithoutMutation()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<DailyLoopException>(() =>
            service.SubmitAttemptAsync(
                Identity,
                repository.Challenge.Id,
                Guid.Parse("30000000-0000-0000-0000-000000000001"),
                Guid.Parse("10000000-0000-0000-0000-000000000007"),
                "ar"));

        Assert.Equal("option_not_in_challenge", exception.Code);
        Assert.Empty(repository.Attempts);
        Assert.Empty(repository.Ledger);
        Assert.Empty(repository.IdempotencyRecords);
        Assert.Null(repository.Progress);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public async Task SubmitAttempt_AppliesPublishedDayContinuityToPersistedProgress(
        bool includeSkippedPublishedDay,
        int expectedCurrentStreak)
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var priorDate = new DateOnly(2026, 8, 26);
        var progress = new UserProgress(repository.User.Id, Now.AddDays(-2));
        progress.ApplyAttempt(
            AttemptScoring.Calculate(isCorrect: false),
            priorDate,
            [priorDate],
            Now.AddDays(-2));
        repository.Progress = progress;
        repository.PublishedDates.Clear();
        repository.PublishedDates.Add(priorDate);
        if (includeSkippedPublishedDay)
        {
            repository.PublishedDates.Add(new DateOnly(2026, 8, 27));
        }

        repository.PublishedDates.Add(repository.DailyMajlis.PublishDate);
        var service = CreateService(repository);

        var result = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.IncorrectOption.Id,
            Guid.NewGuid(),
            "ar");

        Assert.Equal(expectedCurrentStreak, result.Response.Streak.Current);
        Assert.Equal(20, result.Response.Xp.LifetimeTotal);
        Assert.Equal(expectedCurrentStreak, repository.Progress.CurrentStreak);
    }

    [Fact]
    public async Task SubmitAttempt_RecomputesUtcDayAfterWaitingForUserLock()
    {
        var afterMidnight = Now.AddDays(1);
        var repository = new InMemoryDailyLoopRepository(afterMidnight, Identity);
        var timeProvider = new MutableTimeProvider(Now);
        repository.LockUserCallback = () => timeProvider.UtcNow = afterMidnight;
        var service = CreateService(repository, timeProvider);

        var result = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.CorrectOption.Id,
            Guid.NewGuid(),
            "ar");

        Assert.Equal(repository.DailyMajlis.PublishDate, DateOnly.FromDateTime(afterMidnight.UtcDateTime));
        Assert.False(result.IsReplay);
        Assert.Single(repository.Attempts);
        Assert.Equal(afterMidnight, repository.Attempts[0].AttemptedAt);
    }

    [Fact]
    public async Task SubmitAttempt_CrossesUtcMidnightWhileWaitingForPublicationLock_RejectsWithoutAward()
    {
        var afterMidnight = Now.AddDays(1);
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var timeProvider = new MutableTimeProvider(Now);
        repository.GetCurrentPublishedChallengeCallback = () =>
            timeProvider.UtcNow = afterMidnight;
        var service = CreateService(repository, timeProvider);

        var exception = await Assert.ThrowsAsync<DailyLoopException>(() =>
            service.SubmitAttemptAsync(
                Identity,
                repository.Challenge.Id,
                repository.CorrectOption.Id,
                Guid.NewGuid(),
                "ar"));

        Assert.Equal("daily_majlis_unavailable", exception.Code);
        Assert.Empty(repository.Attempts);
        Assert.Empty(repository.Ledger);
        Assert.Empty(repository.IdempotencyRecords);
        Assert.Null(repository.Progress);
    }

    [Fact]
    public async Task SubmitAttempt_WhenLockedAccountRevokedToken_RejectsWithoutAward()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        repository.User.RevokeAuthentication(Identity.IssuedAt);
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<DailyLoopException>(() =>
            service.SubmitAttemptAsync(
                Identity,
                repository.Challenge.Id,
                repository.CorrectOption.Id,
                Guid.NewGuid(),
                "ar"));

        Assert.Equal("forbidden", exception.Code);
        Assert.Empty(repository.Attempts);
        Assert.Empty(repository.Ledger);
        Assert.Empty(repository.IdempotencyRecords);
    }

    [Fact]
    public async Task GetProgress_WhenNoAttemptExists_ReturnsZeroWithoutCreatingProgress()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);

        var progress = await service.GetProgressAsync(Identity);

        Assert.Equal(0, progress.LifetimeXp);
        Assert.Equal(0, progress.CurrentStreak);
        Assert.Equal(0, progress.LongestStreak);
        Assert.Null(progress.LastCompletedPublishDate);
        Assert.Null(repository.Progress);
    }

    [Fact]
    public async Task GetShare_ReturnsOnlySpoilerSafeConfiguredMetadata()
    {
        var repository = new InMemoryDailyLoopRepository(Now, Identity);
        var service = CreateService(repository);
        var submission = await service.SubmitAttemptAsync(
            Identity,
            repository.Challenge.Id,
            repository.CorrectOption.Id,
            Guid.NewGuid(),
            "ar");

        var share = await service.GetShareAsync(Identity, submission.Response.AttemptId);

        Assert.NotNull(share);
        Assert.Equal(repository.DailyMajlis.PublishDate, share.PublishDate);
        Assert.Equal("completed", share.ResultState);
        Assert.Equal(
            "https://share.majlis.test/daily/2026-08-28",
            share.Url);
    }

    private static DailyLoopService CreateService(
        InMemoryDailyLoopRepository repository,
        TimeProvider? timeProvider = null) =>
        new(
            repository,
            timeProvider ?? new FixedTimeProvider(Now),
            new ShareLinkSettings("https://share.majlis.test"));

    private sealed class InMemoryDailyLoopRepository :
        IDailyLoopRepository,
        IDailyLoopTransaction
    {
        private readonly AuthenticatedIdentity _identity;

        public InMemoryDailyLoopRepository(
            DateTimeOffset now,
            AuthenticatedIdentity identity)
        {
            _identity = identity;
            User = UserAccount.Create(
                Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Guid.Parse("40000000-0000-0000-0000-000000000002"),
                identity.Provider,
                identity.Issuer,
                identity.Subject,
                now.AddDays(-10));
            User.CompleteProfile(
                "Test User",
                AgeBand.Adult18Plus,
                "QA",
                "gulf",
                "qa",
                "ar",
                now.AddDays(-10));

            var revisionId = Guid.Parse("50000000-0000-0000-0000-000000000001");
            CorrectOption = new ChallengeOption(
                Guid.Parse("50000000-0000-0000-0000-000000000002"),
                "a",
                isCorrect: true,
                sortOrder: 1);
            IncorrectOption = new ChallengeOption(
                Guid.Parse("50000000-0000-0000-0000-000000000003"),
                "b",
                isCorrect: false,
                sortOrder: 2);
            Challenge = new Challenge(
                Guid.Parse("50000000-0000-0000-0000-000000000004"),
                revisionId,
                ChallengeType.MultipleChoice,
                [CorrectOption, IncorrectOption]);
            Revision = new DailyMajlisRevision(
                revisionId,
                Guid.Parse("50000000-0000-0000-0000-000000000005"),
                1,
                "hospitality",
                ChallengeDifficulty.Easy,
                CardType.Proverb,
                "Verified source notes.",
                createdByUserId: null,
                now.AddDays(-1));
            Revision.SetChallenge(Challenge);
            Revision.AddTranslation(new DailyMajlisTranslation(
                revisionId,
                "ar",
                "العنوان",
                "السؤال",
                "الشرح العربي",
                "النقاش",
                "البطاقة العربية",
                cardTitle: "عنوان البطاقة",
                cardMeaning: "المعنى"));
            Revision.AddTranslation(new DailyMajlisTranslation(
                revisionId,
                "en",
                "English title",
                "English question",
                "English explanation",
                "English discussion",
                "English card",
                cardTitle: "Card title",
                cardMeaning: "Card meaning"));
            foreach (var option in Challenge.Options)
            {
                Revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "ar", "خيار"));
                Revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "en", "Option"));
            }

            Revision.Submit(now.AddHours(-1));
            DailyMajlis = new DailyMajlis(
                Revision.DailyMajlisId,
                DateOnly.FromDateTime(now.UtcDateTime),
                DailyMajlisStatus.Published,
                Revision);
            PublishedDates.Add(DailyMajlis.PublishDate);
        }

        public UserAccount User { get; }

        public DailyMajlis DailyMajlis { get; }

        public DailyMajlisRevision Revision { get; }

        public Challenge Challenge { get; }

        public ChallengeOption CorrectOption { get; }

        public ChallengeOption IncorrectOption { get; }

        public List<UserAttempt> Attempts { get; } = [];

        public List<XpLedgerEntry> Ledger { get; } = [];

        public List<IdempotencyRecord> IdempotencyRecords { get; } = [];

        public UserProgress? Progress { get; set; }

        public List<DateOnly> PublishedDates { get; } = [];

        public Action? LockUserCallback { get; set; }

        public Action? GetCurrentPublishedChallengeCallback { get; set; }

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<IDailyLoopTransaction, CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken) => operation(this, cancellationToken);

        public Task<UserAccount?> LockUserAsync(
            AuthenticatedIdentity identity,
            CancellationToken cancellationToken)
        {
            LockUserCallback?.Invoke();
            return Task.FromResult<UserAccount?>(identity == _identity ? User : null);
        }

        public Task<IdempotencyRecord?> FindIdempotencyAsync(
            Guid userId,
            string scope,
            Guid key,
            CancellationToken cancellationToken) => Task.FromResult(
            IdempotencyRecords.SingleOrDefault(record =>
                record.UserId == userId && record.Scope == scope && record.IdempotencyKey == key));

        public Task<Guid?> FindDailyMajlisIdForChallengeAsync(
            Guid challengeId,
            CancellationToken cancellationToken) => Task.FromResult<Guid?>(
            challengeId == Challenge.Id ? DailyMajlis.Id : null);

        public Task<UserAttempt?> FindAttemptAsync(
            Guid userId,
            Guid dailyMajlisId,
            CancellationToken cancellationToken) => Task.FromResult(
            Attempts.SingleOrDefault(attempt =>
                attempt.UserId == userId && attempt.DailyMajlisId == dailyMajlisId));

        public Task<DailyMajlis?> GetCurrentPublishedChallengeAsync(
            DateOnly publishDate,
            Guid challengeId,
            CancellationToken cancellationToken)
        {
            GetCurrentPublishedChallengeCallback?.Invoke();
            return Task.FromResult<DailyMajlis?>(
                DailyMajlis.PublishDate == publishDate &&
                DailyMajlis.Status == DailyMajlisStatus.Published &&
                Challenge.Id == challengeId
                    ? DailyMajlis
                    : null);
        }

        public Task<UserProgress?> GetProgressForUpdateAsync(
            Guid userId,
            CancellationToken cancellationToken) => Task.FromResult(Progress);

        public Task<IReadOnlyCollection<DateOnly>> GetPublishedDatesAsync(
            DateOnly? after,
            DateOnly through,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<DateOnly>>(
            PublishedDates.Where(date => (!after.HasValue || date >= after.Value) && date <= through)
                .ToArray());

        public void AddAttempt(UserAttempt attempt) => Attempts.Add(attempt);

        public void AddLedgerEntry(XpLedgerEntry entry) => Ledger.Add(entry);

        public void AddProgress(UserProgress progress) => Progress = progress;

        public void AddIdempotencyRecord(IdempotencyRecord record) =>
            IdempotencyRecords.Add(record);

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Guid?> ResolveUserIdAsync(
            AuthenticatedIdentity identity,
            CancellationToken cancellationToken) => Task.FromResult<Guid?>(
            identity == _identity ? User.Id : null);

        public Task<StoredAttemptResult?> GetAttemptResultAsync(
            Guid userId,
            Guid attemptId,
            CancellationToken cancellationToken)
        {
            var attempt = Attempts.SingleOrDefault(item =>
                item.UserId == userId && item.Id == attemptId);
            return Task.FromResult(attempt is null
                ? null
                : new StoredAttemptResult(attempt, DailyMajlis, Revision));
        }

        public Task<Guid?> GetAttemptIdAsync(
            Guid userId,
            Guid dailyMajlisId,
            CancellationToken cancellationToken) => Task.FromResult<Guid?>(Attempts
                .SingleOrDefault(item => item.UserId == userId && item.DailyMajlisId == dailyMajlisId)?.Id);

        public Task<UserProgress?> GetProgressAsync(
            Guid userId,
            CancellationToken cancellationToken) => Task.FromResult(Progress);

        public Task<IReadOnlyList<StoredAttemptHistoryItem>> GetAttemptHistoryAsync(
            Guid userId,
            AttemptHistoryBoundary? boundary,
            int take,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<StoredAttemptHistoryItem>>([]);

        public Task<SubmissionConflictState> ResolveSubmissionConflictAsync(
            AuthenticatedIdentity identity,
            Guid challengeId,
            string scope,
            Guid key,
            CancellationToken cancellationToken) => Task.FromResult(new SubmissionConflictState(
                User.Id,
                IdempotencyRecords.SingleOrDefault(record =>
                    record.Scope == scope && record.IdempotencyKey == key),
                Attempts.SingleOrDefault()));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
