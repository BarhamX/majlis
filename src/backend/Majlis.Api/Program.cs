using Majlis.Application.DailyMajlis;
using Majlis.Infrastructure.DailyMajlis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IDailyMajlisService, DailyMajlisService>();
builder.Services.AddSingleton<IDailyMajlisRepository, SeedDailyMajlisRepository>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
