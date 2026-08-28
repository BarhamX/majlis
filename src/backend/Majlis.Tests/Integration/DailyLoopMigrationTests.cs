using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Majlis.Tests.Integration;

[Collection(PostgreSqlCollection.Name)]
public sealed class DailyLoopMigrationTests(PostgreSqlFixture postgreSql)
{
    private const string DailyLoopPersistenceMigration =
        "20260828114928_AddDailyLoopPersistence";
    private const string PublicationHistoryMigration =
        "20260828124324_RecordDailyMajlisPublicationHistory";

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
        await dbContext.Database.MigrateAsync(DailyLoopPersistenceMigration);
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

    [Theory]
    [InlineData(DailyLoopPersistenceMigration, false)]
    [InlineData(PublicationHistoryMigration, true)]
    public async Task FeatureMigrationHead_WhenDowngradeCrossesBoundary_RejectsWithoutSchemaOrHistoryMutation(
        string featureMigration,
        bool includesPublicationHistory)
    {
        await postgreSql.ResetAsync();
        await using var dbContext = CreateDbContext();
        var migrations = dbContext.Database.GetMigrations().ToArray();
        var boundary = Assert.Single(migrations, migration => migration.EndsWith(
            "_EstablishForwardOnlyLocalizedContentBoundary",
            StringComparison.Ordinal));
        var boundaryIndex = Array.IndexOf(migrations, boundary);
        Assert.True(boundaryIndex > 0);
        await dbContext.Database.MigrateAsync(featureMigration);
        var appliedBefore = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();

        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
            dbContext.Database.MigrateAsync(migrations[boundaryIndex - 1]));

        Assert.Contains("forward-only", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            appliedBefore,
            (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray());
        Assert.Equal(0, await dbContext.UserAttempts.CountAsync());
        Assert.Equal(0, await dbContext.XpLedger.CountAsync());
        Assert.Equal(0, await dbContext.UserProgress.CountAsync());
        Assert.Equal(0, await dbContext.IdempotencyRecords.CountAsync());
        if (includesPublicationHistory)
        {
            Assert.Equal(0, await dbContext.DailyMajlisPublications.CountAsync());
        }
    }

    private MajlisDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql(postgreSql.ConnectionString)
            .Options;
        return new MajlisDbContext(options, TimeProvider.System);
    }
}
