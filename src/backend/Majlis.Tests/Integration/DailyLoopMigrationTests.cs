using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Majlis.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
public sealed class DailyLoopMigrationTests(PostgreSqlFixture postgreSql)
{
    private const string PreviousMigration =
        "20260828064802_EstablishForwardOnlyLocalizedContentBoundary";

    [Fact]
    public async Task FreshDatabase_AppliesDailyLoopMigrationAndCreatesAllTables()
    {
        await postgreSql.ResetAsync();
        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();

        Assert.Contains(
            await dbContext.Database.GetAppliedMigrationsAsync(),
            migration => migration.EndsWith("_AddDailyLoopPersistence", StringComparison.Ordinal));
        Assert.Equal(0, await dbContext.UserAttempts.CountAsync());
        Assert.Equal(0, await dbContext.XpLedger.CountAsync());
        Assert.Equal(0, await dbContext.UserProgress.CountAsync());
        Assert.Equal(0, await dbContext.IdempotencyRecords.CountAsync());
    }

    [Fact]
    public async Task CurrentDatabase_UpgradesForwardWithoutReapplyingExistingMigrations()
    {
        await postgreSql.ResetAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(PreviousMigration);
        var existingMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();

        await dbContext.Database.MigrateAsync();

        var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();
        Assert.Equal(existingMigrations, appliedMigrations.Take(existingMigrations.Length));
        Assert.EndsWith("_AddDailyLoopPersistence", appliedMigrations[^1], StringComparison.Ordinal);
        Assert.Empty(await dbContext.Database.GetPendingMigrationsAsync());
        Assert.Equal(0, await dbContext.UserAttempts.CountAsync());
    }

    private MajlisDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
        return new MajlisDbContext(options, TimeProvider.System);
    }
}
