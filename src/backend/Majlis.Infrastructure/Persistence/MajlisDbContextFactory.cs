using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Majlis.Infrastructure.Persistence;

public sealed class MajlisDbContextFactory : IDesignTimeDbContextFactory<MajlisDbContext>
{
    public MajlisDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__MajlisDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString =
                "Host=localhost;Port=5432;Database=majlis;Username=majlis;Password=majlis-dev";
        }

        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MajlisDbContext(options, TimeProvider.System);
    }
}
