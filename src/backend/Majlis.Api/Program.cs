using Majlis.Api.Authentication;
using Majlis.Api.RateLimiting;
using Majlis.Application.DailyMajlis;
using Majlis.Application.DailyLoop;
using Majlis.Application.Identity;
using Majlis.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new RequiredConsentVersions(
    builder.Configuration["ConsentVersions:Terms"] ?? string.Empty,
    builder.Configuration["ConsentVersions:Privacy"] ?? string.Empty));
builder.Services.AddSingleton(new ShareLinkSettings(
    builder.Configuration["Sharing:PublicHost"] ?? string.Empty));
builder.Services.AddScoped<IDailyMajlisService, DailyMajlisService>();
builder.Services.AddScoped<IDailyLoopService, DailyLoopService>();
builder.Services.AddScoped<IIdentityProfileService, IdentityProfileService>();
builder.Services.AddMajlisAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddDailyAttemptRateLimiting();
builder.Services.AddMajlisInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions());

var initializeDatabase = builder.Configuration.GetValue(
    "DatabaseInitialization:Enabled",
    app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"));
if (initializeDatabase)
{
    await app.Services.InitializeMajlisDatabaseAsync();
}

app.Run();

public partial class Program;
