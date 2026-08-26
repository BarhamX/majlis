using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Majlis.Tests.Integration;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("majlis_tests")
        .WithUsername("majlis")
        .WithPassword("majlis-tests")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public async Task ResetAsync()
    {
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var dbContext = new MajlisDbContext(options, TimeProvider.System);

        await dbContext.Database.ExecuteSqlRawAsync(
            "DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;");
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL integration";
}
