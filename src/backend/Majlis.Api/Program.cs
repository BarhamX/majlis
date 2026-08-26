using Majlis.Application.DailyMajlis;
using Majlis.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IDailyMajlisService, DailyMajlisService>();
builder.Services.AddMajlisInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions());

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    await app.Services.InitializeMajlisDatabaseAsync();
}

app.Run();

public partial class Program;
