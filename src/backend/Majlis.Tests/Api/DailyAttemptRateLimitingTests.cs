using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Majlis.Application.DailyLoop;
using Majlis.Application.Identity;
using Majlis.Contracts.DailyLoop;
using Majlis.Contracts.DailyMajlis;
using Majlis.Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Majlis.Tests.Api;

public sealed class DailyAttemptRateLimitingTests : IDisposable
{
    private readonly DailyAttemptApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-uuid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Submit_WhenIdempotencyKeyIsMissingOrMalformed_ReturnsStableValidationProblem(
        string? idempotencyKey)
    {
        using var client = await CreateCompletedProfileClientAsync("invalid-key-user");

        var response = await SubmitAsync(client, idempotencyKey);
        var body = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("validation_failed", body.GetProperty("code").GetString());
        Assert.Equal(422, body.GetProperty("status").GetInt32());
        Assert.Equal("https://httpstatuses.com/422", body.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
        Assert.Equal(0, _factory.DailyLoopService.SubmitCount);
    }

    [Fact]
    public async Task Submit_WhenAccountExceedsTenRequestsPerMinute_ReturnsRateLimitProblemBeforeSubmission()
    {
        using var client = await CreateCompletedProfileClientAsync("account-rate-user");

        for (var requestNumber = 0; requestNumber < 10; requestNumber++)
        {
            var accepted = await SubmitAsync(client, Guid.NewGuid().ToString("D"));
            Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        }

        var rejected = await SubmitAsync(client, Guid.NewGuid().ToString("D"));
        var body = await ReadJsonAsync(rejected);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
        Assert.True(rejected.Headers.RetryAfter!.Delta > TimeSpan.Zero);
        Assert.Equal("application/problem+json", rejected.Content.Headers.ContentType?.MediaType);
        Assert.Equal("rate_limit_exceeded", body.GetProperty("code").GetString());
        Assert.Equal(429, body.GetProperty("status").GetInt32());
        Assert.Equal("https://httpstatuses.com/429", body.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("traceId").GetString()));
        Assert.Equal(10, _factory.DailyLoopService.SubmitCount);
    }

    [Fact]
    public async Task Submit_WhenIpExceedsSixtyRequestsPerMinute_ReturnsRateLimitProblemBeforeSubmission()
    {
        for (var requestNumber = 0; requestNumber < 60; requestNumber++)
        {
            using var client = await CreateCompletedProfileClientAsync($"ip-rate-user-{requestNumber}");
            var accepted = await SubmitAsync(client, Guid.NewGuid().ToString("D"));
            Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        }

        using var rejectedClient = await CreateCompletedProfileClientAsync("ip-rate-rejected-user");
        var rejected = await SubmitAsync(rejectedClient, Guid.NewGuid().ToString("D"));
        var body = await ReadJsonAsync(rejected);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(rejected.Headers.RetryAfter);
        Assert.Equal("rate_limit_exceeded", body.GetProperty("code").GetString());
        Assert.Equal(60, _factory.DailyLoopService.SubmitCount);
    }

    private async Task<HttpClient> CreateCompletedProfileClientAsync(string subject)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });
        var tokenResponse = await client.PostAsJsonAsync("/api/v1/dev/auth/token", new { subject });
        tokenResponse.EnsureSuccessStatusCode();
        var token = (await ReadJsonAsync(tokenResponse)).GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var bootstrap = await client.PostAsJsonAsync("/api/v1/me/bootstrap", new
        {
            displayName = subject,
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

    private static Task<HttpResponseMessage> SubmitAsync(HttpClient client, string? idempotencyKey)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/challenges/{Guid.NewGuid():D}/attempts")
        {
            Content = JsonContent.Create(new { selectedOptionId = Guid.NewGuid() }),
        };
        if (idempotencyKey is not null)
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        return client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    private sealed class DailyAttemptApiFactory : WebApplicationFactory<Program>
    {
        public InMemoryUserAccountRepository UserAccounts { get; } = new();

        public RecordingDailyLoopService DailyLoopService { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:MajlisDatabase",
                "Host=localhost;Database=not_used;Username=not_used;Password=not_used");
            builder.UseSetting("Authentication:Mode", "Test");
            builder.UseSetting("DatabaseInitialization:Enabled", "false");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUserAccountRepository>();
                services.AddSingleton<IUserAccountRepository>(UserAccounts);
                services.RemoveAll<IDailyLoopService>();
                services.AddSingleton<IDailyLoopService>(DailyLoopService);
            });
        }
    }

    private sealed class InMemoryUserAccountRepository : IUserAccountRepository
    {
        private readonly List<UserAccount> _users = [];

        public Task<UserAccount?> FindByIdentityAsync(
            ExternalIdentityProvider provider,
            string issuer,
            string subject,
            CancellationToken cancellationToken) => Task.FromResult(
            _users.SingleOrDefault(user => user.Identities.Any(identity =>
                identity.Provider == provider &&
                identity.Issuer == issuer &&
                identity.Subject == subject)));

        public void Add(UserAccount user) => _users.Add(user);

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingDailyLoopService : IDailyLoopService
    {
        private int _submitCount;

        public int SubmitCount => Volatile.Read(ref _submitCount);

        public Task<AttemptSubmissionResult> SubmitAttemptAsync(
            AuthenticatedIdentity identity,
            Guid challengeId,
            Guid selectedOptionId,
            Guid idempotencyKey,
            string? acceptLanguage,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _submitCount);
            return Task.FromResult(new AttemptSubmissionResult(
                new AttemptResultResponse(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new DateOnly(2026, 8, 28),
                    IsCorrect: false,
                    Guid.NewGuid(),
                    "explanation",
                    new CulturalCardResponse("proverb", null, "card", null, null, null),
                    new AttemptXpResponse(10, 0, 10, 10),
                    new AttemptStreakResponse(1, 1, Updated: true),
                    Guid.NewGuid(),
                    "ar"),
                IsReplay: false));
        }

        public Task<AttemptResultResponse?> GetAttemptAsync(
            AuthenticatedIdentity identity,
            Guid attemptId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<AttemptHistoryResponse> GetAttemptHistoryAsync(
            AuthenticatedIdentity identity,
            string? cursor,
            int limit,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<UserProgressResponse> GetProgressAsync(
            AuthenticatedIdentity identity,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<AttemptShareResponse?> GetShareAsync(
            AuthenticatedIdentity identity,
            Guid attemptId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<DailyMajlisUserStateResponse> GetTodayStateAsync(
            AuthenticatedIdentity identity,
            Guid dailyMajlisId,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
