using Majlis.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Majlis.Tests.Infrastructure;

public sealed class ForwardOnlyDailyLoopMigrationTests
{
    [Theory]
    [InlineData("daily-loop persistence", typeof(TestableAddDailyLoopPersistence))]
    [InlineData("publication history", typeof(TestableRecordDailyMajlisPublicationHistory))]
    public void Down_RejectsBeforeAddingAnyDestructiveOperation(
        string expectedBoundary,
        Type migrationType)
    {
        var migration = (ITestableMigration)Activator.CreateInstance(migrationType)!;
        var migrationBuilder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");

        var exception = Assert.Throws<NotSupportedException>(() =>
            migration.InvokeDown(migrationBuilder));

        Assert.Contains(expectedBoundary, exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(migrationBuilder.Operations);
    }

    private interface ITestableMigration
    {
        void InvokeDown(MigrationBuilder migrationBuilder);
    }

    private sealed class TestableAddDailyLoopPersistence :
        AddDailyLoopPersistence,
        ITestableMigration
    {
        public void InvokeDown(MigrationBuilder migrationBuilder) => Down(migrationBuilder);
    }

    private sealed class TestableRecordDailyMajlisPublicationHistory :
        RecordDailyMajlisPublicationHistory,
        ITestableMigration
    {
        public void InvokeDown(MigrationBuilder migrationBuilder) => Down(migrationBuilder);
    }
}
