using Majlis.Api.Authentication;
using Majlis.Application.DailyMajlis;
using Majlis.Application.Identity;
using Majlis.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IDailyMajlisService, DailyMajlisService>();
builder.Services.AddScoped<IIdentityProfileService, IdentityProfileService>();
builder.Services.AddMajlisAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddMajlisInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
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
