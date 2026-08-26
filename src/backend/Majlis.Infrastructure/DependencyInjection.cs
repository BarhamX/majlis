using Majlis.Application.DailyMajlis;
using Majlis.Infrastructure.DailyMajlis;
using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Majlis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMajlisInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MajlisDatabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'MajlisDatabase' is required. " +
                "Set ConnectionStrings__MajlisDatabase for non-development environments.");
        }

        services.AddDbContext<MajlisDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IDailyMajlisRepository, EfDailyMajlisRepository>();
        services.AddScoped<DailyMajlisDatabaseInitializer>();
        services.AddHealthChecks().AddDbContextCheck<MajlisDbContext>("postgresql");

        return services;
    }

    public static async Task InitializeMajlisDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DailyMajlisDatabaseInitializer>();
        await initializer.InitializeAsync(cancellationToken);
    }
}
