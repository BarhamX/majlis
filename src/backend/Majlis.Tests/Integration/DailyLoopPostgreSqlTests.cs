using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Majlis.Application.DailyLoop;
using Majlis.Application.Identity;
using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Identity;
using Majlis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Majlis.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
public sealed class DailyLoopPostgreSqlTests(PostgreSqlFixture postgreSql) : IAsyncLifetime
{
    private const string SeedEnglishExplanation =
        "This proverb reflects hospitality as honor and responsibility.";
    private const string CorrectedArabicExplanation = "شرح مصحح";
    private static readonly DateTimeOffset FirstDay =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    private DailyLoopApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await postgreSql.ResetAsync();
        StartFactory(FirstDay);
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData(false, 10, 0, 10)]
    [InlineData(true, 10, 5, 15)]
    public async Task Submit_FirstAcceptedAttempt_CommitsExactAwardOnce(
        bool correct,
        int completionXp,
        int correctnessXp,
        int awardedXp)
    {
        using var client = await CreateAuthenticatedClientAsync("exact-award-user");
        var today = await GetTodayAsync(client);
        var selectedOptionId = correct ? today.CorrectOptionId : today.IncorrectOptionId;

        var response = await SubmitAsync(
            client,
            today.ChallengeId,
            selectedOptionId,
            Guid.NewGuid());
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(correct, body.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(completionXp, body.GetProperty("xp").GetProperty("completion").GetInt32());
        Assert.Equal(correctnessXp, body.GetProperty("xp").GetProperty("correctness").GetInt32());
        Assert.Equal(awardedXp, body.GetProperty("xp").GetProperty("awarded").GetInt32());
        Assert.Equal(awardedXp, body.GetProperty("xp").GetProperty("lifetimeTotal").GetInt64());
        Assert.Equal(1, body.GetProperty("streak").GetProperty("current").GetInt32());

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var attempt = await dbContext.UserAttempts.SingleAsync();
        Assert.Equal(awardedXp, attempt.CompletionXp + attempt.CorrectnessXp);
        Assert.Equal(awardedXp, attempt.LifetimeXpAfter);
        Assert.Equal(awardedXp, (await dbContext.XpLedger.SingleAsync()).Amount);
        Assert.Equal(awardedXp, (await dbContext.UserProgress.SingleAsync()).LifetimeXp);
        Assert.Equal(1, await dbContext.IdempotencyRecords.CountAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-uuid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Submit_MissingOrMalformedIdempotencyKey_ReturnsValidationWithoutMutation(
        string? idempotencyKey)
    {
        using var client = await CreateAuthenticatedClientAsync("invalid-key-database-user");
        var today = await GetTodayAsync(client);

        var response = await SubmitWithIdempotencyKeyAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            idempotencyKey);

        await AssertProblemCodeAsync(
            response,
            HttpStatusCode.UnprocessableEntity,
            "validation_failed");
        await AssertDailyLoopRowCountsAsync(attempts: 0, ledger: 0, progress: 0, idempotency: 0);
    }

    [Fact]
    public async Task Submit_UnsupportedLocale_FallsBackToArabicAndStoresFallback()
    {
        using var client = await CreateAuthenticatedClientAsync("locale-fallback-user");
        var today = await GetTodayAsync(client);

        var response = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid(),
            "fr-CA");
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("ar", body.GetProperty("resultLocale").GetString());
        Assert.Contains("ar", response.Content.Headers.ContentLanguage);
        await using var scope = _factory.Services.CreateAsyncScope();
        Assert.Equal(
            "ar",
            (await scope.ServiceProvider.GetRequiredService<MajlisDbContext>()
                .UserAttempts.SingleAsync()).ResultLocale);
    }

    [Fact]
    public async Task Submit_AccountRateLimitRejectsBeforeAnyAdditionalDailyLoopMutation()
    {
        using var client = await CreateAuthenticatedClientAsync("database-account-rate-user");
        var today = await GetTodayAsync(client);

        var first = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        for (var requestNumber = 1; requestNumber < 10; requestNumber++)
        {
            var completed = await SubmitAsync(
                client,
                today.ChallengeId,
                today.CorrectOptionId,
                Guid.NewGuid());
            await AssertProblemCodeAsync(
                completed,
                HttpStatusCode.Conflict,
                "attempt_already_completed");
        }

        var rejected = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid());

        await AssertRateLimitProblemAsync(rejected);
        await AssertDailyLoopRowCountsAsync(attempts: 1, ledger: 1, progress: 1, idempotency: 1);
    }

    [Fact]
    public async Task Submit_IpRateLimitRejectsSixtyFirstAccountBeforeAnyAdditionalMutation()
    {
        TodayIds? today = null;
        for (var requestNumber = 0; requestNumber < 60; requestNumber++)
        {
            using var client = await CreateAuthenticatedClientAsync($"database-ip-rate-user-{requestNumber}");
            today ??= await GetTodayAsync(client);
            var accepted = await SubmitAsync(
                client,
                today.ChallengeId,
                today.CorrectOptionId,
                Guid.NewGuid());
            Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        }

        using var rejectedClient = await CreateAuthenticatedClientAsync("database-ip-rate-rejected-user");
        var rejected = await SubmitAsync(
            rejectedClient,
            today!.ChallengeId,
            today!.CorrectOptionId,
            Guid.NewGuid());

        await AssertRateLimitProblemAsync(rejected);
        await AssertDailyLoopRowCountsAsync(attempts: 60, ledger: 60, progress: 60, idempotency: 60);
    }

    [Fact]
    public async Task Submit_HistoricalChallenge_ReturnsUnavailableWithoutMutation()
    {
        using var firstClient = await CreateAuthenticatedClientAsync("historical-user");
        var historical = await GetTodayAsync(firstClient);
        firstClient.Dispose();
        StartFactory(FirstDay.AddDays(1));
        using var currentClient = await CreateAuthenticatedClientAsync("historical-user");

        var response = await SubmitAsync(
            currentClient,
            historical.ChallengeId,
            historical.CorrectOptionId,
            Guid.NewGuid());

        await AssertProblemCodeAsync(response, HttpStatusCode.NotFound, "daily_majlis_unavailable");
        await AssertDailyLoopRowCountsAsync(attempts: 0, ledger: 0, progress: 0, idempotency: 0);
    }

    [Fact]
    public async Task Submit_OptionFromAnotherChallenge_ReturnsOwnershipErrorWithoutMutation()
    {
        using var client = await CreateAuthenticatedClientAsync("wrong-option-user");
        var today = await GetTodayAsync(client);

        var response = await SubmitAsync(
            client,
            today.ChallengeId,
            Guid.NewGuid(),
            Guid.NewGuid());

        await AssertProblemCodeAsync(
            response,
            HttpStatusCode.UnprocessableEntity,
            "option_not_in_challenge");
        await AssertDailyLoopRowCountsAsync(attempts: 0, ledger: 0, progress: 0, idempotency: 0);
    }

    [Fact]
    public async Task Submit_SameKeyAndPayload_ReplaysOriginalStoredResultWithOk()
    {
        using var client = await CreateAuthenticatedClientAsync("replay-user");
        var today = await GetTodayAsync(client);
        var key = Guid.NewGuid();

        var first = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            key,
            acceptLanguage: "en-US");
        var replay = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            key,
            acceptLanguage: "ar");
        var firstJson = await first.Content.ReadAsStringAsync();
        var replayJson = await replay.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(firstJson, replayJson);
        Assert.Equal("en", (await ReadJsonAsync(first)).GetProperty("resultLocale").GetString());
        var replayBody = await ReadJsonAsync(replay);
        Assert.Equal("en", replayBody.GetProperty("resultLocale").GetString());
        Assert.Contains("en", replay.Content.Headers.ContentLanguage);
        Assert.Equal(SeedEnglishExplanation, replayBody.GetProperty("explanation").GetString());
        await AssertDailyLoopRowCountsAsync(attempts: 1, ledger: 1, progress: 1, idempotency: 1);
    }

    [Fact]
    public async Task Submit_SameKeyWithChangedPayload_ReturnsIdempotencyConflict()
    {
        using var client = await CreateAuthenticatedClientAsync("reused-key-user");
        var today = await GetTodayAsync(client);
        var key = Guid.NewGuid();
        (await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            key)).EnsureSuccessStatusCode();

        var response = await SubmitAsync(
            client,
            today.ChallengeId,
            today.IncorrectOptionId,
            key);

        await AssertProblemCodeAsync(
            response,
            HttpStatusCode.Conflict,
            "idempotency_key_reused");
        await AssertDailyLoopRowCountsAsync(attempts: 1, ledger: 1, progress: 1, idempotency: 1);
    }

    [Fact]
    public async Task Submit_DifferentKeyAfterCompletion_ReturnsExistingAttemptId()
    {
        using var client = await CreateAuthenticatedClientAsync("completed-user");
        var today = await GetTodayAsync(client);
        var accepted = await SubmitAsync(
            client,
            today.ChallengeId,
            today.IncorrectOptionId,
            Guid.NewGuid());
        var acceptedBody = await ReadJsonAsync(accepted);

        var response = await SubmitAsync(
            client,
            today.ChallengeId,
            today.IncorrectOptionId,
            Guid.NewGuid());
        var problem = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("attempt_already_completed", problem.GetProperty("code").GetString());
        Assert.Equal(
            acceptedBody.GetProperty("attemptId").GetGuid(),
            problem.GetProperty("attemptId").GetGuid());
        await AssertDailyLoopRowCountsAsync(attempts: 1, ledger: 1, progress: 1, idempotency: 1);
    }

    [Fact]
    public async Task Submit_ConsecutivePublishedDays_IncrementsStreak()
    {
        var first = await SubmitForDayAsync("consecutive-user", correct: false);
        Assert.Equal(1, first.GetProperty("streak").GetProperty("current").GetInt32());
        StartFactory(FirstDay.AddDays(1));

        var second = await SubmitForDayAsync("consecutive-user", correct: true);

        Assert.Equal(2, second.GetProperty("streak").GetProperty("current").GetInt32());
        Assert.Equal(25, second.GetProperty("xp").GetProperty("lifetimeTotal").GetInt64());
    }

    [Fact]
    public async Task Submit_MissingUnpublishedContentDay_DoesNotBreakStreak()
    {
        await SubmitForDayAsync("exempt-day-user", correct: false);
        StartFactory(FirstDay.AddDays(2));

        var second = await SubmitForDayAsync("exempt-day-user", correct: false);

        Assert.Equal(2, second.GetProperty("streak").GetProperty("current").GetInt32());
    }

    [Fact]
    public async Task Submit_SkippedPublishedContentDay_ResetsStreak()
    {
        await SubmitForDayAsync("reset-user", correct: false);
        StartFactory(FirstDay.AddDays(1));
        using (var skippedDayClient = await CreateAuthenticatedClientAsync("reset-user"))
        {
            Assert.Equal(HttpStatusCode.OK, (await skippedDayClient.GetAsync(
                "/api/v1/daily-majlis/today")).StatusCode);
        }

        StartFactory(FirstDay.AddDays(2));

        var thirdDay = await SubmitForDayAsync("reset-user", correct: false);

        Assert.Equal(1, thirdDay.GetProperty("streak").GetProperty("current").GetInt32());
        Assert.Equal(1, thirdDay.GetProperty("streak").GetProperty("longest").GetInt32());
    }

    [Fact]
    public async Task Submit_SkippedDayPublishedThenUnpublished_StillResetsStreak()
    {
        await SubmitForDayAsync("unpublished-reset-user", correct: false);
        StartFactory(FirstDay.AddDays(1));
        using (var skippedClient = await CreateAuthenticatedClientAsync("unpublished-reset-user"))
        {
            await GetTodayAsync(skippedClient);
        }

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
            await dbContext.DailyMajlis
                .Where(item => item.PublishDate == DateOnly.FromDateTime(FirstDay.AddDays(1).UtcDateTime))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.Status, DailyMajlisStatus.Unpublished)
                    .SetProperty(item => item.PublishedRevisionId, (Guid?)null));
        }

        StartFactory(FirstDay.AddDays(2));
        var thirdDay = await SubmitForDayAsync("unpublished-reset-user", correct: false);

        Assert.Equal(1, thirdDay.GetProperty("streak").GetProperty("current").GetInt32());
    }

    [Fact]
    public async Task Submit_CrossesUtcMidnightWhileWaitingForUserLock_UsesPostLockDay()
    {
        const string subject = "midnight-lock-user";
        using var client = await CreateAuthenticatedClientAsync(subject);
        var today = await GetTodayAsync(client);
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var userId = await dbContext.UserIdentities
            .Where(item => item.Subject == subject)
            .Select(item => item.UserId)
            .SingleAsync();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.Users.FromSqlInterpolated($$"""
            SELECT * FROM "Users" WHERE "Id" = {{userId}} FOR UPDATE
            """).SingleAsync();

        var submission = SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid());
        await Task.Delay(200);
        Assert.False(submission.IsCompleted);
        _factory.Clock.UtcNow = FirstDay.AddDays(1);
        await transaction.CommitAsync();

        await AssertProblemCodeAsync(
            await submission,
            HttpStatusCode.NotFound,
            "daily_majlis_unavailable");
        await AssertDailyLoopRowCountsAsync(0, 0, 0, 0);
    }

    [Fact]
    public async Task Submit_RacingUnpublish_HoldsPublicationDecisionUntilTransitionCommits()
    {
        using var client = await CreateAuthenticatedClientAsync("unpublish-race-user");
        var today = await GetTodayAsync(client);
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.DailyMajlis
            .Where(item => item.Id == today.DailyMajlisId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Status, DailyMajlisStatus.Unpublished)
                .SetProperty(item => item.PublishedRevisionId, (Guid?)null));

        var submission = SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid());
        await Task.Delay(200);
        Assert.False(submission.IsCompleted);
        await transaction.CommitAsync();

        await AssertProblemCodeAsync(
            await submission,
            HttpStatusCode.NotFound,
            "daily_majlis_unavailable");
        await AssertDailyLoopRowCountsAsync(0, 0, 0, 0);
    }

    [Fact]
    public async Task Submit_WhenLedgerPersistenceFails_RollsBackEveryDailyLoopRow()
    {
        const string subject = "rollback-user";
        using var client = await CreateAuthenticatedClientAsync(subject);
        var today = await GetTodayAsync(client);
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION fail_test_xp_insert() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                RAISE EXCEPTION 'test-only XP persistence failure';
            END;
            $$;
            CREATE TRIGGER fail_test_xp_insert
            BEFORE INSERT ON "XpLedger"
            FOR EACH ROW EXECUTE FUNCTION fail_test_xp_insert();
            """);
        var service = scope.ServiceProvider.GetRequiredService<IDailyLoopService>();
        var identity = new AuthenticatedIdentity(
            ExternalIdentityProvider.Test,
            "https://test.majlis.local",
            subject,
            DateTimeOffset.UtcNow.AddMinutes(-1));

        await Assert.ThrowsAnyAsync<Exception>(() => service.SubmitAttemptAsync(
            identity,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid(),
            "ar"));

        dbContext.ChangeTracker.Clear();
        Assert.Equal(0, await dbContext.UserAttempts.CountAsync());
        Assert.Equal(0, await dbContext.XpLedger.CountAsync());
        Assert.Equal(0, await dbContext.UserProgress.CountAsync());
        Assert.Equal(0, await dbContext.IdempotencyRecords.CountAsync());
    }

    [Fact]
    public async Task AcceptedAttempt_SurvivesApplicationRestart()
    {
        const string subject = "restart-user";
        using var client = await CreateAuthenticatedClientAsync(subject);
        var today = await GetTodayAsync(client);
        var accepted = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid(),
            "en");
        var acceptedJson = await accepted.Content.ReadAsStringAsync();
        using var acceptedDocument = JsonDocument.Parse(acceptedJson);
        var attemptId = acceptedDocument.RootElement.GetProperty("attemptId").GetGuid();
        Assert.Equal("en", acceptedDocument.RootElement.GetProperty("resultLocale").GetString());
        Assert.Equal(
            SeedEnglishExplanation,
            acceptedDocument.RootElement.GetProperty("explanation").GetString());
        client.Dispose();

        StartFactory(FirstDay);
        using var restartedClient = await CreateAuthenticatedClientAsync(subject);
        var result = await restartedClient.GetAsync($"/api/v1/attempts/{attemptId}");
        var progress = await restartedClient.GetFromJsonAsync<JsonElement>("/api/v1/me/progress");
        var restartedToday = await ReadJsonAsync(
            await restartedClient.GetAsync("/api/v1/daily-majlis/today"));

        var resultJson = await result.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(acceptedJson, resultJson);
        Assert.Contains("en", result.Content.Headers.ContentLanguage);
        using var resultDocument = JsonDocument.Parse(resultJson);
        Assert.Equal("en", resultDocument.RootElement.GetProperty("resultLocale").GetString());
        Assert.Equal(
            SeedEnglishExplanation,
            resultDocument.RootElement.GetProperty("explanation").GetString());
        Assert.Equal(15, progress.GetProperty("lifetimeXp").GetInt64());
        Assert.True(restartedToday.GetProperty("userState").GetProperty("hasAttempted").GetBoolean());
        Assert.Equal(
            attemptId,
            restartedToday.GetProperty("userState").GetProperty("attemptId").GetGuid());
    }

    [Fact]
    public async Task Submit_WhenSameKeyRequestsRace_ConvergesOnOneReplayableResult()
    {
        const string subject = "same-key-race-user";
        using var firstClient = await CreateAuthenticatedClientAsync(subject);
        using var secondClient = await CreateAuthenticatedClientAsync(subject);
        var today = await GetTodayAsync(firstClient);
        var key = Guid.NewGuid();

        var responses = await Task.WhenAll(
            SubmitAsync(firstClient, today.ChallengeId, today.CorrectOptionId, key),
            SubmitAsync(secondClient, today.ChallengeId, today.CorrectOptionId, key));

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.OK);
        var firstBody = await responses[0].Content.ReadAsStringAsync();
        var secondBody = await responses[1].Content.ReadAsStringAsync();
        Assert.Equal(firstBody, secondBody);
        await AssertDailyLoopRowCountsAsync(attempts: 1, ledger: 1, progress: 1, idempotency: 1);
    }

    [Fact]
    public async Task Submit_WhenDifferentKeyRequestsRace_ConvergesOnCompletedConflict()
    {
        const string subject = "different-key-race-user";
        using var firstClient = await CreateAuthenticatedClientAsync(subject);
        using var secondClient = await CreateAuthenticatedClientAsync(subject);
        var today = await GetTodayAsync(firstClient);

        var responses = await Task.WhenAll(
            SubmitAsync(firstClient, today.ChallengeId, today.IncorrectOptionId, Guid.NewGuid()),
            SubmitAsync(secondClient, today.ChallengeId, today.IncorrectOptionId, Guid.NewGuid()));

        var accepted = Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        var conflict = Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        var acceptedBody = await ReadJsonAsync(accepted);
        var conflictBody = await ReadJsonAsync(conflict);
        Assert.Equal("attempt_already_completed", conflictBody.GetProperty("code").GetString());
        Assert.Equal(
            acceptedBody.GetProperty("attemptId").GetGuid(),
            conflictBody.GetProperty("attemptId").GetGuid());
        await AssertDailyLoopRowCountsAsync(attempts: 1, ledger: 1, progress: 1, idempotency: 1);
    }

    [Fact]
    public async Task ResultRead_AfterCorrection_UsesStoredRevisionLocaleForever()
    {
        using var client = await CreateAuthenticatedClientAsync("correction-user");
        var today = await GetTodayAsync(client);
        var accepted = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid(),
            "en-US");
        var acceptedJson = await accepted.Content.ReadAsStringAsync();
        using var acceptedDocument = JsonDocument.Parse(acceptedJson);
        var acceptedBody = acceptedDocument.RootElement;
        var attemptId = acceptedBody.GetProperty("attemptId").GetGuid();
        var originalRevisionId = acceptedBody.GetProperty("contentRevisionId").GetGuid();
        await ReplacePublishedRevisionAsync(today.DailyMajlisId, originalRevisionId);
        client.DefaultRequestHeaders.AcceptLanguage.Clear();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ar");

        var result = await client.GetAsync($"/api/v1/attempts/{attemptId}");
        var resultJson = await result.Content.ReadAsStringAsync();
        using var resultDocument = JsonDocument.Parse(resultJson);
        var resultBody = resultDocument.RootElement;

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(acceptedJson, resultJson);
        Assert.Contains("en", result.Content.Headers.ContentLanguage);
        Assert.Equal(originalRevisionId, resultBody.GetProperty("contentRevisionId").GetGuid());
        Assert.Equal("en", resultBody.GetProperty("resultLocale").GetString());
        Assert.Equal(SeedEnglishExplanation, resultBody.GetProperty("explanation").GetString());
        Assert.DoesNotContain(CorrectedArabicExplanation, resultJson, StringComparison.Ordinal);
        Assert.Equal(15, resultBody.GetProperty("xp").GetProperty("lifetimeTotal").GetInt64());
        Assert.Equal(1, resultBody.GetProperty("streak").GetProperty("current").GetInt32());
    }

    [Fact]
    public async Task ResultRead_AfterLaterProgressAndUnpublishing_ReturnsOriginalSnapshots()
    {
        const string subject = "snapshot-user";
        using var firstClient = await CreateAuthenticatedClientAsync(subject);
        var firstToday = await GetTodayAsync(firstClient);
        var firstResponse = await SubmitAsync(
            firstClient,
            firstToday.ChallengeId,
            firstToday.IncorrectOptionId,
            Guid.NewGuid());
        var firstAttemptId = (await ReadJsonAsync(firstResponse)).GetProperty("attemptId").GetGuid();
        firstClient.Dispose();
        StartFactory(FirstDay.AddDays(1));
        await SubmitForDayAsync(subject, correct: true);
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
            await dbContext.DailyMajlis
                .Where(item => item.Id == firstToday.DailyMajlisId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.Status, DailyMajlisStatus.Unpublished)
                    .SetProperty(item => item.PublishedRevisionId, (Guid?)null));
        }

        using var secondClient = await CreateAuthenticatedClientAsync(subject);
        var result = await ReadJsonAsync(
            await secondClient.GetAsync($"/api/v1/attempts/{firstAttemptId}"));

        Assert.Equal(10, result.GetProperty("xp").GetProperty("lifetimeTotal").GetInt64());
        Assert.Equal(1, result.GetProperty("streak").GetProperty("current").GetInt32());
        Assert.Equal(1, result.GetProperty("streak").GetProperty("longest").GetInt32());
    }

    [Fact]
    public async Task AttemptReads_AreOwnedAndNonEnumerating()
    {
        using var owner = await CreateAuthenticatedClientAsync("attempt-owner");
        var today = await GetTodayAsync(owner);
        var accepted = await SubmitAsync(
            owner,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid());
        var attemptId = (await ReadJsonAsync(accepted)).GetProperty("attemptId").GetGuid();
        using var other = await CreateAuthenticatedClientAsync("attempt-other-user");

        var nonOwned = await other.GetAsync($"/api/v1/attempts/{attemptId}");
        var missing = await other.GetAsync($"/api/v1/attempts/{Guid.NewGuid()}");
        var nonOwnedShare = await other.GetAsync($"/api/v1/attempts/{attemptId}/share");
        var missingShare = await other.GetAsync($"/api/v1/attempts/{Guid.NewGuid()}/share");

        await AssertProblemCodeAsync(nonOwned, HttpStatusCode.NotFound, "attempt_not_found");
        await AssertProblemCodeAsync(missing, HttpStatusCode.NotFound, "attempt_not_found");
        await AssertProblemCodeAsync(nonOwnedShare, HttpStatusCode.NotFound, "attempt_not_found");
        await AssertProblemCodeAsync(missingShare, HttpStatusCode.NotFound, "attempt_not_found");
    }

    [Fact]
    public async Task Progress_WhenNoAttemptExists_ReturnsZeroWithoutCreatingRow()
    {
        using var client = await CreateAuthenticatedClientAsync("zero-progress-user");

        var response = await client.GetAsync("/api/v1/me/progress");
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, body.GetProperty("lifetimeXp").GetInt64());
        Assert.Equal(0, body.GetProperty("currentStreak").GetInt32());
        Assert.Equal(0, body.GetProperty("longestStreak").GetInt32());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("lastCompletedPublishDate").ValueKind);
        await using var scope = _factory.Services.CreateAsyncScope();
        Assert.Equal(
            0,
            await scope.ServiceProvider.GetRequiredService<MajlisDbContext>().UserProgress.CountAsync());
    }

    [Fact]
    public async Task History_UsesExclusiveNewestFirstOpaqueCursor()
    {
        const string subject = "history-user";
        await SubmitForDayAsync(subject, correct: false);
        StartFactory(FirstDay.AddDays(1));
        await SubmitForDayAsync(subject, correct: true);
        StartFactory(FirstDay.AddDays(2));
        await SubmitForDayAsync(subject, correct: false);
        using var client = await CreateAuthenticatedClientAsync(subject);

        var firstPage = await client.GetFromJsonAsync<JsonElement>("/api/v1/me/attempts?limit=2");
        var firstItems = firstPage.GetProperty("items");
        var cursor = firstPage.GetProperty("nextCursor").GetString();

        Assert.Equal(2, firstItems.GetArrayLength());
        Assert.Equal("2026-08-28", firstItems[0].GetProperty("publishDate").GetString());
        Assert.Equal("2026-08-27", firstItems[1].GetProperty("publishDate").GetString());
        Assert.False(string.IsNullOrWhiteSpace(cursor));
        Assert.DoesNotContain("2026-08-27", cursor, StringComparison.Ordinal);

        client.Dispose();
        StartFactory(FirstDay.AddDays(3));
        await SubmitForDayAsync(subject, correct: true);
        using var continuationClient = await CreateAuthenticatedClientAsync(subject);

        var secondPage = await continuationClient.GetFromJsonAsync<JsonElement>(
            $"/api/v1/me/attempts?limit=2&cursor={Uri.EscapeDataString(cursor!)}");
        var secondItems = secondPage.GetProperty("items");
        Assert.Equal(1, secondItems.GetArrayLength());
        Assert.Equal("2026-08-26", secondItems[0].GetProperty("publishDate").GetString());
        Assert.Equal(JsonValueKind.Null, secondPage.GetProperty("nextCursor").ValueKind);
    }

    [Fact]
    public async Task Share_ReturnsConfiguredSpoilerSafeContractOnly()
    {
        using var client = await CreateAuthenticatedClientAsync("share-user");
        var today = await GetTodayAsync(client);
        var accepted = await SubmitAsync(
            client,
            today.ChallengeId,
            today.CorrectOptionId,
            Guid.NewGuid());
        var attemptId = (await ReadJsonAsync(accepted)).GetProperty("attemptId").GetGuid();

        var response = await client.GetAsync($"/api/v1/attempts/{attemptId}/share");
        var json = await response.Content.ReadAsStringAsync();
        using var body = JsonDocument.Parse(json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            ["body", "imageAlt", "publishDate", "resultState", "title", "url"],
            body.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray());
        Assert.Equal("completed", body.RootElement.GetProperty("resultState").GetString());
        Assert.Equal(
            "https://share.majlis.test/daily/2026-08-26",
            body.RootElement.GetProperty("url").GetString());
        Assert.DoesNotContain("correct", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("option", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("identity", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("xp", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("streak", json, StringComparison.OrdinalIgnoreCase);
    }

    private void StartFactory(DateTimeOffset utcNow)
    {
        _factory?.Dispose();
        _factory = new DailyLoopApiFactory(postgreSql.ConnectionString, utcNow);
        using var client = CreateClient();
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string subject)
    {
        var client = CreateClient();
        var tokenResponse = await client.PostAsJsonAsync(
            "/api/v1/dev/auth/token",
            new { subject });
        tokenResponse.EnsureSuccessStatusCode();
        var token = (await ReadJsonAsync(tokenResponse)).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var bootstrap = await client.PostAsJsonAsync(
            "/api/v1/me/bootstrap",
            new
            {
                displayName = "Test User",
                ageBand = "18_plus",
                countryCode = "QA",
                regionCode = "gulf",
                dialectCode = "qa",
                locale = "ar",
                acceptedTermsVersion = "2026-08-26",
                acceptedPrivacyVersion = "2026-08-26",
            });
        bootstrap.EnsureSuccessStatusCode();
        return client;
    }

    private static async Task<TodayIds> GetTodayAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/daily-majlis/today");
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonAsync(response);
        var challenge = body.GetProperty("challenge");
        var options = challenge.GetProperty("options");
        return new TodayIds(
            body.GetProperty("dailyMajlisId").GetGuid(),
            challenge.GetProperty("id").GetGuid(),
            options[0].GetProperty("id").GetGuid(),
            options[1].GetProperty("id").GetGuid());
    }

    private static Task<HttpResponseMessage> SubmitAsync(
        HttpClient client,
        Guid challengeId,
        Guid optionId,
        Guid key,
        string acceptLanguage = "ar")
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/challenges/{challengeId}/attempts")
        {
            Content = JsonContent.Create(new { selectedOptionId = optionId }),
        };
        request.Headers.Add("Idempotency-Key", key.ToString("D"));
        request.Headers.AcceptLanguage.ParseAdd(acceptLanguage);
        return client.SendAsync(request);
    }

    private static Task<HttpResponseMessage> SubmitWithIdempotencyKeyAsync(
        HttpClient client,
        Guid challengeId,
        Guid optionId,
        string? idempotencyKey)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/challenges/{challengeId}/attempts")
        {
            Content = JsonContent.Create(new { selectedOptionId = optionId }),
        };
        if (idempotencyKey is not null)
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        return client.SendAsync(request);
    }

    private async Task<JsonElement> SubmitForDayAsync(string subject, bool correct)
    {
        using var client = await CreateAuthenticatedClientAsync(subject);
        var today = await GetTodayAsync(client);
        var response = await SubmitAsync(
            client,
            today.ChallengeId,
            correct ? today.CorrectOptionId : today.IncorrectOptionId,
            Guid.NewGuid());
        response.EnsureSuccessStatusCode();
        return await ReadJsonAsync(response);
    }

    private async Task AssertDailyLoopRowCountsAsync(
        int attempts,
        int ledger,
        int progress,
        int idempotency)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        Assert.Equal(attempts, await dbContext.UserAttempts.CountAsync());
        Assert.Equal(ledger, await dbContext.XpLedger.CountAsync());
        Assert.Equal(progress, await dbContext.UserProgress.CountAsync());
        Assert.Equal(idempotency, await dbContext.IdempotencyRecords.CountAsync());
    }

    private async Task ReplacePublishedRevisionAsync(Guid dailyMajlisId, Guid originalRevisionId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var dailyMajlis = await dbContext.DailyMajlis
            .Include(item => item.Publication)
            .SingleAsync(item => item.Id == dailyMajlisId);
        var revisionId = Guid.NewGuid();
        var challenge = new Challenge(
            Guid.NewGuid(),
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.NewGuid(), "a", true, 1),
                new ChallengeOption(Guid.NewGuid(), "b", false, 2),
            ]);
        var replacement = new DailyMajlisRevision(
            revisionId,
            dailyMajlisId,
            2,
            "hospitality",
            ChallengeDifficulty.Easy,
            CardType.Proverb,
            "Replacement source notes.",
            createdByUserId: null,
            FirstDay.AddHours(1),
            originalRevisionId);
        replacement.SetChallenge(challenge);
        replacement.AddTranslation(new DailyMajlisTranslation(
            revisionId,
            "ar",
            "عنوان مصحح",
            "سؤال مصحح",
            "شرح مصحح",
            "نقاش مصحح",
            "بطاقة مصححة"));
        foreach (var option in challenge.Options)
        {
            replacement.AddOptionTranslation(new ChallengeOptionTranslation(
                option.Id,
                "ar",
                option.IsCorrect ? "صحيح" : "خطأ"));
        }

        replacement.Submit(FirstDay.AddHours(2));
        dbContext.DailyMajlisRevisions.Add(replacement);
        await dbContext.SaveChangesAsync();
        dailyMajlis.Publish(replacement);
        await dbContext.SaveChangesAsync();
    }

    private static async Task AssertProblemCodeAsync(
        HttpResponseMessage response,
        HttpStatusCode status,
        string code)
    {
        var body = await ReadJsonAsync(response);
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(code, body.GetProperty("code").GetString());
    }

    private static async Task AssertRateLimitProblemAsync(HttpResponseMessage response)
    {
        var body = await ReadJsonAsync(response);
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.NotNull(response.Headers.RetryAfter);
        Assert.True(response.Headers.RetryAfter!.Delta > TimeSpan.Zero);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("rate_limit_exceeded", body.GetProperty("code").GetString());
        Assert.Equal(429, body.GetProperty("status").GetInt32());
        Assert.Equal("https://httpstatuses.com/429", body.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    private sealed class DailyLoopApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public DailyLoopApiFactory(string connectionString, DateTimeOffset utcNow)
        {
            _connectionString = connectionString;
            Clock = new MutableTimeProvider(utcNow);
        }

        public MutableTimeProvider Clock { get; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:MajlisDatabase", _connectionString);
            builder.UseSetting("Authentication:Mode", "Test");
            builder.UseSetting("Sharing:PublicHost", "https://share.majlis.test");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Clock);
            });
        }
    }

    private sealed record TodayIds(
        Guid DailyMajlisId,
        Guid ChallengeId,
        Guid CorrectOptionId,
        Guid IncorrectOptionId);

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private long _utcTicks = utcNow.UtcTicks;

        public DateTimeOffset UtcNow
        {
            get => new(Interlocked.Read(ref _utcTicks), TimeSpan.Zero);
            set => Interlocked.Exchange(ref _utcTicks, value.UtcTicks);
        }

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
