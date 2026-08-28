using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Majlis.Domain.DailyMajlis;
using Majlis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Majlis.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
public sealed class DailyMajlisApiTests(PostgreSqlFixture postgreSql) : IAsyncLifetime
{
    private static readonly Guid LegacySeedDailyMajlisId =
        Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset TestNow =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    private MajlisApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await postgreSql.ResetAsync();
        _factory = new MajlisApiFactory(postgreSql.ConnectionString, TestNow);

        using var client = CreateClient();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetToday_WhenUnauthenticated_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/v1/daily-majlis/today");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetToday_WhenProfileIsIncomplete_ReturnsForbidden()
    {
        using var client = await CreateTokenClientAsync("incomplete-profile-user");

        var response = await client.GetAsync("/api/v1/daily-majlis/today");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetToday_WhenPublishedContentExists_ReturnsPersistedSpoilerSafePayload()
    {
        using var client = await CreateAuthenticatedClientAsync("daily-content-user");

        var response = await client.GetAsync("/api/v1/daily-majlis/today");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("correct", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("currentStreak", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("publishDate", json, StringComparison.Ordinal);
        Assert.Contains("topicCode", json, StringComparison.Ordinal);
        Assert.Contains("discussionPrompt", json, StringComparison.Ordinal);
        Assert.Contains("hasAttempted", json, StringComparison.Ordinal);
        Assert.Contains("Vary", response.Headers.ToString(), StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal("2026-08-26", root.GetProperty("publishDate").GetString());

        var options = root.GetProperty("challenge").GetProperty("options");
        Assert.Equal(2, options.GetArrayLength());
        Assert.Equal("gulf", root.GetProperty("challenge").GetProperty("regionCode").GetString());
        Assert.Equal("إكرام الضيف أمانة", options[0].GetProperty("text").GetString());
        Assert.Equal("لا ينبغي للضيف أن يطيل", options[1].GetProperty("text").GetString());
        Assert.Contains("ar", response.Content.Headers.ContentLanguage);
    }

    [Fact]
    public async Task GetToday_AcceptLanguageFallsBackFromRegionalArabicAndServesEnglishWhenRequested()
    {
        using var client = await CreateAuthenticatedClientAsync("localized-content-user");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ar-QA");

        var arabicResponse = await client.GetAsync("/api/v1/daily-majlis/today");
        var arabicJson = await arabicResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.OK, arabicResponse.StatusCode);
        Assert.Contains("ar", arabicResponse.Content.Headers.ContentLanguage);
        Assert.Equal("الضيف قبل البيت", arabicJson.GetProperty("title").GetString());

        client.DefaultRequestHeaders.AcceptLanguage.Clear();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");
        var englishResponse = await client.GetAsync("/api/v1/daily-majlis/today");
        var englishJson = await englishResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.OK, englishResponse.StatusCode);
        Assert.Contains("en", englishResponse.Content.Headers.ContentLanguage);
        Assert.Equal("The Guest Before the House", englishJson.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetToday_WhenPublishedContentDoesNotExist_ReturnsSafeNotFound()
    {
        using var client = await CreateAuthenticatedClientAsync("no-content-user");
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
            await dbContext.DailyMajlis.ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Status, DailyMajlisStatus.Unpublished)
                .SetProperty(item => item.PublishedRevisionId, (Guid?)null));
        }

        var response = await client.GetAsync("/api/v1/daily-majlis/today");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Today's Majlis is not available yet.", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Health_WhenPostgreSqlIsAvailable_ReturnsHealthy()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task Initializer_WhenRunMoreThanOnce_RemainsIdempotent()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();

        await initializer.InitializeAsync();
        await initializer.InitializeAsync();

        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        Assert.Equal(1, await dbContext.DailyMajlis.CountAsync());
        Assert.Equal(1, await dbContext.Challenges.CountAsync());
        Assert.Equal(2, await dbContext.ChallengeOptions.CountAsync());
        Assert.Equal(1, await dbContext.DailyMajlisRevisions.CountAsync());
        Assert.Equal(2, await dbContext.DailyMajlisTranslations.CountAsync());
        Assert.Equal(4, await dbContext.ChallengeOptionTranslations.CountAsync());
        Assert.True((await dbContext.DailyMajlisRevisions.SingleAsync()).IsImmutable);
    }

    [Fact]
    public async Task Initializer_WhenUpgradingMutablePublishedSeed_SealsAReplacementRevision()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        await ResetDailyMajlisContentAsync(dbContext);
        var legacySeed = CreateDailyMajlis(
            LegacySeedDailyMajlisId,
            new DateOnly(2026, 8, 26),
            DailyMajlisStatus.Published);
        await SavePublicationGraphAsync(dbContext, legacySeed);
        await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE \"DailyMajlisRevisions\" SET \"SubmittedAt\" = NULL;");
        dbContext.ChangeTracker.Clear();

        var initializer = scope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();
        await initializer.InitializeAsync();

        dbContext.ChangeTracker.Clear();
        var repaired = await dbContext.DailyMajlis
            .Include(item => item.PublishedRevision)
            .ThenInclude(revision => revision!.Translations)
            .Include(item => item.PublishedRevision)
            .ThenInclude(revision => revision!.Challenge)
            .ThenInclude(challenge => challenge!.Options)
            .ThenInclude(option => option.Translations)
            .SingleAsync(item => item.Id == LegacySeedDailyMajlisId);
        Assert.Equal(DailyMajlisStatus.Published, repaired.Status);
        Assert.NotNull(repaired.PublishedRevision);
        Assert.True(repaired.PublishedRevision.IsImmutable);
        Assert.True(repaired.PublishedRevision.IsCompleteForServing());
        Assert.Equal(2, await dbContext.DailyMajlisRevisions.CountAsync());
    }

    [Fact]
    public async Task Initializer_WhenConcurrentStartsRepairUnpublishedLegacySeed_Converges()
    {
        await using (var setupScope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<MajlisDbContext>();
            await ResetDailyMajlisContentAsync(dbContext);
            dbContext.DailyMajlis.Add(new DailyMajlis(
                LegacySeedDailyMajlisId,
                new DateOnly(2026, 8, 26)));
            await dbContext.SaveChangesAsync();
        }

        await using var firstScope = _factory.Services.CreateAsyncScope();
        await using var secondScope = _factory.Services.CreateAsyncScope();
        var first = firstScope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();
        var second = secondScope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();

        await Task.WhenAll(first.InitializeAsync(), second.InitializeAsync());

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verification = verificationScope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var repaired = await verification.DailyMajlis
            .Include(item => item.PublishedRevision)
            .SingleAsync(item => item.Id == LegacySeedDailyMajlisId);
        Assert.Equal(DailyMajlisStatus.Published, repaired.Status);
        Assert.NotNull(repaired.PublishedRevision);
        Assert.True(repaired.PublishedRevision.IsImmutable);
        Assert.Equal(1, await verification.DailyMajlisRevisions.CountAsync());
        Assert.Equal(1, await verification.DailyMajlisPublications.CountAsync(publication =>
            publication.DailyMajlisId == LegacySeedDailyMajlisId &&
            publication.PublishDate == new DateOnly(2026, 8, 26)));
    }

    [Fact]
    public async Task Initializer_WhenScheduledContentExists_PreservesEditorialContent()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var scheduled = await dbContext.DailyMajlis.SingleAsync();
        var revisionId = scheduled.PublishedRevisionId;
        dbContext.Entry(scheduled).Property(item => item.Status).CurrentValue = DailyMajlisStatus.Scheduled;
        dbContext.Entry(scheduled).Property(item => item.ScheduledRevisionId).CurrentValue = revisionId;
        dbContext.Entry(scheduled).Property(item => item.PublishedRevisionId).CurrentValue = null;
        await dbContext.SaveChangesAsync();

        var initializer = scope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();
        await initializer.InitializeAsync();

        dbContext.ChangeTracker.Clear();
        var actual = await dbContext.DailyMajlis.SingleAsync();
        Assert.Equal(DailyMajlisStatus.Scheduled, actual.Status);
        Assert.Equal(revisionId, actual.ScheduledRevisionId);
        Assert.Null(actual.PublishedRevisionId);
        Assert.Equal(1, await dbContext.DailyMajlisRevisions.CountAsync());
    }

    [Fact]
    public async Task Initializer_WhenUtcDayAdvances_PreservesPriorPublishedDay()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var nextDay = new DateTimeOffset(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);
        var initializer = new DailyMajlisDatabaseInitializer(
            dbContext,
            new FixedTimeProvider(nextDay));

        await initializer.InitializeAsync();

        dbContext.ChangeTracker.Clear();
        var publishedDates = await dbContext.DailyMajlis
            .Where(item => item.Status == DailyMajlisStatus.Published)
            .OrderBy(item => item.PublishDate)
            .Select(item => item.PublishDate)
            .ToArrayAsync();
        Assert.Equal(
            [new DateOnly(2026, 8, 26), new DateOnly(2026, 8, 27)],
            publishedDates);
    }

    [Fact]
    public async Task Initializer_WhenConcurrentStartsRace_ConvergesOnOnePublishedDay()
    {
        await using (var setupScope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<MajlisDbContext>();
            await dbContext.DailyMajlis.ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Status, DailyMajlisStatus.Unpublished)
                .SetProperty(item => item.PublishedRevisionId, (Guid?)null));
        }

        await using var firstScope = _factory.Services.CreateAsyncScope();
        await using var secondScope = _factory.Services.CreateAsyncScope();
        var first = firstScope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();
        var second = secondScope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();

        await Task.WhenAll(first.InitializeAsync(), second.InitializeAsync());

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verification = verificationScope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        Assert.Equal(1, await verification.DailyMajlis.CountAsync(item =>
            item.PublishDate == new DateOnly(2026, 8, 26) &&
            item.Status == DailyMajlisStatus.Published));
        Assert.Equal(1, await verification.DailyMajlisPublications.CountAsync(item =>
            item.PublishDate == new DateOnly(2026, 8, 26)));
    }

    [Fact]
    public async Task Database_WhenSecondOfficialMajlisUsesSameDate_RejectsDuplicate()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var duplicateDailyMajlis = CreateDailyMajlis(
            new DateOnly(2026, 8, 26),
            DailyMajlisStatus.Scheduled);
        dbContext.DailyMajlis.Add(duplicateDailyMajlis);
        dbContext.Entry(duplicateDailyMajlis)
            .Reference(item => item.ScheduledRevision)
            .CurrentValue = null;
        dbContext.Entry(duplicateDailyMajlis)
            .Property(item => item.ScheduledRevisionId)
            .CurrentValue = null;

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ForwardMigration_PreservesLegacyEnglishAndExplicitlyUnpublishesIt()
    {
        _factory.Dispose();
        await postgreSql.ResetAsync();
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
        await using var dbContext = new MajlisDbContext(options, TimeProvider.System);
        await dbContext.Database.MigrateAsync("20260826193542_AddIdentityProfileFoundation");
        await dbContext.Database.ExecuteSqlRawAsync("""
            INSERT INTO "Challenges" ("Id", "QuestionText", "Type", "Difficulty", "Region", "Topic", "Explanation", "SourceNotes", "ReviewStatus", "CreatedAt")
            VALUES ('51000000-0000-0000-0000-000000000001', 'Legacy question', 'multipleChoice', 'easy', 'gulf', 'hospitality', 'Legacy explanation', 'Legacy source', 'reviewed', now());
            INSERT INTO "ChallengeOptions" ("Id", "Text", "IsCorrect", "SortOrder", "ChallengeId")
            VALUES ('52000000-0000-0000-0000-000000000001', 'Legacy answer', true, 1, '51000000-0000-0000-0000-000000000001'),
                   ('52000000-0000-0000-0000-000000000002', 'Legacy distractor', false, 2, '51000000-0000-0000-0000-000000000001');
            INSERT INTO "DailyMajlis" ("Id", "PublishDate", "Title", "Topic", "ChallengeId", "DiscussionQuestion", "Status", "CreatedAt", "UpdatedAt")
            VALUES ('53000000-0000-0000-0000-000000000001', '2026-08-26', 'Legacy title', 'hospitality', '51000000-0000-0000-0000-000000000001', 'Legacy discussion', 'published', now(), now());
            """);

        await dbContext.Database.MigrateAsync();

        var legacy = await dbContext.DailyMajlisRevisions
            .Include(revision => revision.Translations)
            .SingleAsync(revision => revision.DailyMajlisId == Guid.Parse("53000000-0000-0000-0000-000000000001"));
        var daily = await dbContext.DailyMajlis.SingleAsync(item => item.Id == legacy.DailyMajlisId);
        Assert.Equal("en", Assert.Single(legacy.Translations).Locale);
        Assert.Equal(DailyMajlisStatus.Unpublished, daily.Status);
        Assert.Null(daily.PublishedRevisionId);
    }

    [Fact]
    public async Task ForwardOnlyLocalizedContentBoundary_WhenDowngradeRequested_RejectsWithoutSchemaLoss()
    {
        _factory.Dispose();
        await postgreSql.ResetAsync();
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
        await using var dbContext = new MajlisDbContext(options, TimeProvider.System);
        var migrations = dbContext.Database.GetMigrations().ToArray();
        var boundary = Assert.Single(migrations, id => id.EndsWith(
            "_EstablishForwardOnlyLocalizedContentBoundary",
            StringComparison.Ordinal));
        var boundaryIndex = Array.IndexOf(migrations, boundary);
        Assert.True(boundaryIndex > 0);
        await dbContext.Database.MigrateAsync();

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            dbContext.Database.MigrateAsync(migrations[boundaryIndex - 1]));

        Assert.Contains("forward-only boundary", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, await dbContext.DailyMajlisRevisions.CountAsync());
        Assert.Contains(boundary, await dbContext.Database.GetAppliedMigrationsAsync());
    }

    private static DailyMajlis CreateDailyMajlis(
        DateOnly publishDate,
        DailyMajlisStatus status) => CreateDailyMajlis(
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            publishDate,
            status);

    private static DailyMajlis CreateDailyMajlis(
        Guid dailyId,
        DateOnly publishDate,
        DailyMajlisStatus status)
    {
        var revisionId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        var challenge = new Challenge(
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
            revisionId,
            ChallengeType.MultipleChoice,
            [
                new ChallengeOption(Guid.Parse("30000000-0000-0000-0000-000000000003"), "First option", true, 1),
                new ChallengeOption(Guid.Parse("30000000-0000-0000-0000-000000000004"), "Second option", false, 2),
            ]);
        var revision = new DailyMajlisRevision(
            revisionId, dailyId, 1, "hospitality", ChallengeDifficulty.Easy,
            CardType.Proverb, "Integration-test source notes.", null, DateTimeOffset.UtcNow);
        revision.SetChallenge(challenge);
        revision.AddTranslation(new DailyMajlisTranslation(revisionId, "ar", "عنوان", "سؤال", "شرح", "نقاش", "بطاقة"));
        foreach (var option in challenge.Options)
        {
            revision.AddOptionTranslation(new ChallengeOptionTranslation(option.Id, "ar", "خيار"));
        }
        revision.Submit(DateTimeOffset.UtcNow);

        return new DailyMajlis(dailyId, publishDate, status, revision);
    }

    private static async Task ResetDailyMajlisContentAsync(MajlisDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"DailyMajlis\" CASCADE;");
        dbContext.ChangeTracker.Clear();
    }

    private static async Task SavePublicationGraphAsync(
        MajlisDbContext dbContext,
        DailyMajlis dailyMajlis)
    {
        var revision = dailyMajlis.PublishedRevision!;
        dbContext.DailyMajlis.Add(dailyMajlis);
        dbContext.Entry(dailyMajlis).Reference(item => item.PublishedRevision).CurrentValue = null;
        dbContext.Entry(dailyMajlis).Property(item => item.PublishedRevisionId).CurrentValue = null;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        await dbContext.SaveChangesAsync();
        dailyMajlis.Publish(revision);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string subject)
    {
        var client = await CreateTokenClientAsync(subject);

        var bootstrapResponse = await client.PostAsJsonAsync(
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
        bootstrapResponse.EnsureSuccessStatusCode();
        return client;
    }

    private async Task<HttpClient> CreateTokenClientAsync(string subject)
    {
        var client = CreateClient();
        var tokenResponse = await client.PostAsJsonAsync(
            "/api/v1/dev/auth/token",
            new { subject });
        tokenResponse.EnsureSuccessStatusCode();

        using var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenJson.RootElement.GetProperty("accessToken").GetString());
        return client;
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });

    private sealed class MajlisApiFactory(
        string connectionString,
        DateTimeOffset utcNow) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:MajlisDatabase", connectionString);
            builder.UseSetting("Authentication:Mode", "Test");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(utcNow));
            });
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
