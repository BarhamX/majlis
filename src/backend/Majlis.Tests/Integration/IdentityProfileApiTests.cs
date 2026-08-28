using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Majlis.Application.Identity;
using Majlis.Domain.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Majlis.Tests.Integration;

public sealed class IdentityProfileApiTests : IDisposable
{
    private readonly IdentityApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetProfile_WhenUnauthenticated_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/v1/me/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetToday_WhenUnauthenticated_ReturnsUnauthorizedBeforeDataAccess()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/api/v1/daily-majlis/today");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WhenBearerTokenIsMalformed_ReturnsUnauthorized()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "not-a-token");

        var response = await client.GetAsync("/api/v1/me/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetToday_WhenIdentityHasNoProfile_ReturnsForbiddenBeforeDataAccess()
    {
        using var client = await CreateTokenClientAsync("no-profile");

        var response = await client.GetAsync("/api/v1/daily-majlis/today");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains(
            "profile_incomplete",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Bootstrap_WhenTestIdentityIsValid_CreatesPrivateProfileIdempotently()
    {
        using var client = await CreateTokenClientAsync("bootstrap-user");
        var request = CreateBootstrapRequest("  مريم  ", "18_plus", "global_weekly");

        var first = await client.PostAsJsonAsync("/api/v1/me/bootstrap", request);
        var second = await client.PostAsJsonAsync("/api/v1/me/bootstrap", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        using var firstJson = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        Assert.Equal("مريم", firstJson.RootElement.GetProperty("displayName").GetString());
        Assert.Equal("QA", firstJson.RootElement.GetProperty("countryCode").GetString());
        Assert.Equal("private", firstJson.RootElement.GetProperty("leaderboardVisibility").GetString());
        Assert.False(firstJson.RootElement.GetProperty("preferences").GetProperty("reminderEnabled").GetBoolean());
        Assert.Equal("test", firstJson.RootElement.GetProperty("linkedProviders")[0].GetString());

        var user = Assert.Single(_factory.Repository.Users);
        Assert.Single(user.Identities);
        Assert.NotNull(user.Profile);
        Assert.NotNull(user.Preferences);
        Assert.Equal(2, user.Consents.Count);
    }

    [Fact]
    public async Task Bootstrap_WhenAgeIsUnderThirteen_ReturnsValidationErrorWithoutPersistingUser()
    {
        using var client = await CreateTokenClientAsync("underage-user");

        var response = await client.PostAsJsonAsync(
            "/api/v1/me/bootstrap",
            CreateBootstrapRequest("Young User", "under_13", "private"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("age_not_eligible", body, StringComparison.Ordinal);

        Assert.Empty(_factory.Repository.Users);
    }

    [Fact]
    public async Task Profiles_AreResolvedFromAuthenticatedIdentityAndRemainIsolated()
    {
        using var mariam = await CreateTokenClientAsync("mariam");
        using var omar = await CreateTokenClientAsync("omar");
        Assert.Equal(
            HttpStatusCode.Created,
            (await mariam.PostAsJsonAsync(
                "/api/v1/me/bootstrap",
                CreateBootstrapRequest("مريم", "18_plus", "private"))).StatusCode);
        Assert.Equal(
            HttpStatusCode.Created,
            (await omar.PostAsJsonAsync(
                "/api/v1/me/bootstrap",
                CreateBootstrapRequest("عمر", "18_plus", "private"))).StatusCode);

        using var mariamProfile = JsonDocument.Parse(
            await (await mariam.GetAsync("/api/v1/me/profile")).Content.ReadAsStringAsync());
        using var omarProfile = JsonDocument.Parse(
            await (await omar.GetAsync("/api/v1/me/profile")).Content.ReadAsStringAsync());

        Assert.Equal("مريم", mariamProfile.RootElement.GetProperty("displayName").GetString());
        Assert.Equal("عمر", omarProfile.RootElement.GetProperty("displayName").GetString());
        Assert.NotEqual(
            mariamProfile.RootElement.GetProperty("userId").GetGuid(),
            omarProfile.RootElement.GetProperty("userId").GetGuid());
    }

    [Fact]
    public async Task UpdateProfile_WhenMinorOptsIntoLeaderboard_ReturnsValidationError()
    {
        using var client = await CreateTokenClientAsync("minor-user");
        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync(
                "/api/v1/me/bootstrap",
                CreateBootstrapRequest("نورة", "13_17", "private"))).StatusCode);

        var response = await client.PutAsJsonAsync(
            "/api/v1/me/profile",
            CreateBootstrapRequest("نورة", "13_17", "global_weekly"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(
            "leaderboard_age_ineligible",
            await response.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateProfile_WhenAdultOptsIn_ReturnsTheUpdatedPrivateProfile()
    {
        using var client = await CreateTokenClientAsync("adult-user");
        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync(
                "/api/v1/me/bootstrap",
                CreateBootstrapRequest("Mariam", "18_plus", "private"))).StatusCode);

        var response = await client.PutAsJsonAsync(
            "/api/v1/me/profile",
            CreateBootstrapRequest("مريم الجديدة", "18_plus", "global_weekly"));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("مريم الجديدة", body.RootElement.GetProperty("displayName").GetString());
        Assert.Equal(
            "global_weekly",
            body.RootElement.GetProperty("leaderboardVisibility").GetString());
    }

    [Fact]
    public async Task RequestDeletion_RevokesCredentialAndMakesProfileUnavailable()
    {
        using var client = await CreateTokenClientAsync("delete-user");
        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync(
                "/api/v1/me/bootstrap",
                CreateBootstrapRequest("Delete Me", "18_plus", "private"))).StatusCode);

        var first = await client.PostAsJsonAsync(
            "/api/v1/me/deletion-requests",
            new { confirmation = "delete_my_account" });
        var second = await client.PostAsJsonAsync(
            "/api/v1/me/deletion-requests",
            new { confirmation = "delete_my_account" });

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/me/profile")).StatusCode);
    }

    [Fact]
    public async Task RevokeAllSessions_MakesTheCurrentCredentialUnavailable()
    {
        using var client = await CreateTokenClientAsync("revoke-user");
        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync(
                "/api/v1/me/bootstrap",
                CreateBootstrapRequest("Revoke Me", "18_plus", "private"))).StatusCode);

        var revokeResponse = await client.PostAsync(
            "/api/v1/me/sessions/revoke-all",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/me/profile")).StatusCode);
    }

    private async Task<HttpClient> CreateTokenClientAsync(string subject)
    {
        var client = CreateClient();
        var tokenResponse = await client.PostAsJsonAsync(
            "/api/v1/dev/auth/token",
            new { subject });
        tokenResponse.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });

    private static object CreateBootstrapRequest(
        string displayName,
        string ageBand,
        string leaderboardVisibility) => new
        {
            displayName,
            ageBand,
            countryCode = "qa",
            regionCode = "gulf",
            dialectCode = "qa",
            locale = "ar",
            leaderboardVisibility,
            acceptedTermsVersion = "2026-08-26",
            acceptedPrivacyVersion = "2026-08-26",
        };

    private sealed class IdentityApiFactory : WebApplicationFactory<Program>
    {
        public InMemoryUserAccountRepository Repository { get; } = new();

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
                services.AddSingleton<IUserAccountRepository>(Repository);
            });
        }
    }

    private sealed class InMemoryUserAccountRepository : IUserAccountRepository
    {
        public List<UserAccount> Users { get; } = [];

        public Task<UserAccount?> FindByIdentityAsync(
            ExternalIdentityProvider provider,
            string issuer,
            string subject,
            CancellationToken cancellationToken) => Task.FromResult(
                Users.SingleOrDefault(user => user.Identities.Any(identity =>
                    identity.Provider == provider &&
                    identity.Issuer == issuer &&
                    identity.Subject == subject)));

        public void Add(UserAccount user) => Users.Add(user);

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
