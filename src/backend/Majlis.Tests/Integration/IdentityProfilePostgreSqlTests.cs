using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Majlis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Majlis.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
public sealed class IdentityProfilePostgreSqlTests(PostgreSqlFixture postgreSql) : IAsyncLifetime
{
    private IdentityPostgreSqlApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await postgreSql.ResetAsync();
        _factory = new IdentityPostgreSqlApiFactory(postgreSql.ConnectionString);

        using var client = CreateClient();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Bootstrap_PersistsTheCompleteIdentityProfileFoundation()
    {
        using var client = await CreateTokenClientAsync("persisted-user");

        var response = await client.PostAsJsonAsync(
            "/api/v1/me/bootstrap",
            BootstrapRequest("مريم"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        var user = await dbContext.Users
            .Include(item => item.Identities)
            .Include(item => item.Profile)
            .Include(item => item.Preferences)
            .Include(item => item.Consents)
            .SingleAsync();

        Assert.Equal("مريم", user.Profile!.DisplayName);
        Assert.Equal("QA", user.Profile.CountryCode);
        Assert.Equal("test", user.Identities.Single().Provider.ToString().ToLowerInvariant());
        Assert.False(user.Preferences.ReminderEnabled);
        Assert.Equal(2, user.Consents.Count);
    }

    [Fact]
    public async Task Bootstrap_WhenRequestsRace_ConvergesOnOneUser()
    {
        using var firstClient = await CreateTokenClientAsync("concurrent-user");
        using var secondClient = await CreateTokenClientAsync("concurrent-user");

        var responses = await Task.WhenAll(
            firstClient.PostAsJsonAsync("/api/v1/me/bootstrap", BootstrapRequest("مريم")),
            secondClient.PostAsJsonAsync("/api/v1/me/bootstrap", BootstrapRequest("مريم")));

        Assert.All(responses, response => Assert.Contains(
            response.StatusCode,
            new[] { HttpStatusCode.Created, HttpStatusCode.OK }));

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MajlisDbContext>();
        Assert.Equal(1, await dbContext.Users.CountAsync());
        Assert.Equal(1, await dbContext.UserIdentities.CountAsync());
    }

    private HttpClient CreateClient() => _factory.CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });

    private async Task<HttpClient> CreateTokenClientAsync(string subject)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/v1/dev/auth/token",
            new { subject });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            json.RootElement.GetProperty("accessToken").GetString());
        return client;
    }

    private static object BootstrapRequest(string displayName) => new
    {
        displayName,
        ageBand = "18_plus",
        countryCode = "QA",
        regionCode = "gulf",
        dialectCode = "qa",
        locale = "ar",
        acceptedTermsVersion = "2026-08-26",
        acceptedPrivacyVersion = "2026-08-26",
    };

    private sealed class IdentityPostgreSqlApiFactory(
        string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:MajlisDatabase", connectionString);
            builder.UseSetting("Authentication:Mode", "Test");
        }
    }
}
