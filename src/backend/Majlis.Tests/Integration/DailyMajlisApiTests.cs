using System.Net;
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
    private static readonly DateTimeOffset TestNow =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    private MajlisApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await postgreSql.ResetAsync();
        _factory = new MajlisApiFactory(postgreSql.ConnectionString, TestNow);

        using var client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetToday_WhenPublishedContentExists_ReturnsPersistedSpoilerSafePayload()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/daily-majlis/today");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("correct", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", json, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal("2026-08-26", root.GetProperty("date").GetString());

        var options = root.GetProperty("challenge").GetProperty("options");
        Assert.Equal(2, options.GetArrayLength());
        Assert.Equal("A guest is honored as a trust", options[0].GetProperty("text").GetString());
        Assert.Equal("A guest should not stay long", options[1].GetProperty("text").GetString());
    }

    [Fact]
    public async Task GetToday_WhenPublishedContentDoesNotExist_ReturnsSafeNotFound()
    {
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
            await dbContext.DailyMajlis.ExecuteDeleteAsync();
        }

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/daily-majlis/today");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Today's Majlis is not available yet.", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Health_WhenPostgreSqlIsAvailable_ReturnsHealthy()
    {
        using var client = _factory.CreateClient();

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
    }

    [Fact]
    public async Task Database_WhenSecondOfficialMajlisUsesSameDate_RejectsDuplicate()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        dbContext.DailyMajlis.Add(CreateDailyMajlis(
            new DateOnly(2026, 8, 26),
            DailyMajlisStatus.Scheduled));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    private static DailyMajlis CreateDailyMajlis(
        DateOnly publishDate,
        DailyMajlisStatus status)
    {
        return new DailyMajlis(
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            publishDate,
            "Another Daily Majlis",
            "hospitality",
            new Challenge(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "Another question?",
                ChallengeType.MultipleChoice,
                ChallengeDifficulty.Easy,
                "panArab",
                "hospitality",
                "Another explanation.",
                "Integration-test source notes.",
                ContentReviewStatus.Reviewed,
                [
                    new ChallengeOption(
                        Guid.Parse("30000000-0000-0000-0000-000000000003"),
                        "First option",
                        isCorrect: true,
                        sortOrder: 1),
                    new ChallengeOption(
                        Guid.Parse("30000000-0000-0000-0000-000000000004"),
                        "Second option",
                        isCorrect: false,
                        sortOrder: 2),
                ]),
            "Another discussion question?",
            status);
    }

    private sealed class MajlisApiFactory(
        string connectionString,
        DateTimeOffset utcNow) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:MajlisDatabase", connectionString);
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
