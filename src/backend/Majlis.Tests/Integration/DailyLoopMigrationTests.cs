using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Majlis.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
public sealed class DailyLoopMigrationTests(PostgreSqlFixture postgreSql)
{
    private const string PreviousMigration =
        "20260828114928_AddDailyLoopPersistence";

    [Fact]
    public async Task FreshDatabase_AppliesDailyLoopMigrationAndCreatesAllTables()
    {
        await postgreSql.ResetAsync();
        await using var dbContext = CreateDbContext();

        await dbContext.Database.MigrateAsync();

        Assert.Contains(
            await dbContext.Database.GetAppliedMigrationsAsync(),
            migration => migration.EndsWith(
                "_RecordDailyMajlisPublicationHistory",
                StringComparison.Ordinal));
        Assert.Equal(0, await dbContext.UserAttempts.CountAsync());
        Assert.Equal(0, await dbContext.XpLedger.CountAsync());
        Assert.Equal(0, await dbContext.UserProgress.CountAsync());
        Assert.Equal(0, await dbContext.IdempotencyRecords.CountAsync());
        Assert.Equal(0, await dbContext.DailyMajlisPublications.CountAsync());
    }

    [Fact]
    public async Task CurrentDatabase_UpgradesForwardWithoutReapplyingExistingMigrations()
    {
        await postgreSql.ResetAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(PreviousMigration);
        var legacyId = Guid.NewGuid();
        var legacyDate = new DateOnly(2026, 8, 20);
        var legacyUpdatedAt = new DateTimeOffset(2026, 8, 21, 8, 0, 0, TimeSpan.Zero);
        await dbContext.Database.ExecuteSqlInterpolatedAsync($$"""
            INSERT INTO "DailyMajlis"
                ("Id", "PublishDate", "Status", "ScheduledRevisionId", "PublishedRevisionId", "CreatedAt", "UpdatedAt")
            VALUES
                ({{legacyId}}, {{legacyDate}}, 'unpublished', NULL, NULL, {{legacyUpdatedAt}}, {{legacyUpdatedAt}})
            """);
        var existingMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();

        await dbContext.Database.MigrateAsync();

        var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();
        Assert.Equal(existingMigrations, appliedMigrations.Take(existingMigrations.Length));
        Assert.EndsWith(
            "_RecordDailyMajlisPublicationHistory",
            appliedMigrations[^1],
            StringComparison.Ordinal);
        Assert.Empty(await dbContext.Database.GetPendingMigrationsAsync());
        Assert.Equal(0, await dbContext.UserAttempts.CountAsync());
        var publication = await dbContext.DailyMajlisPublications.SingleAsync();
        Assert.Equal(legacyId, publication.DailyMajlisId);
        Assert.Equal(legacyDate, publication.PublishDate);
        Assert.Equal(legacyUpdatedAt, publication.PublishedAt);
    }

    private MajlisDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
        return new MajlisDbContext(options, TimeProvider.System);
    }
}
