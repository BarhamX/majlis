using Majlis.Domain.DailyMajlis;
using Majlis.Domain.Progress;
using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Majlis.Tests.Infrastructure;

public sealed class DailyLoopPersistenceModelTests
{
    private readonly IModel _model;

    public DailyLoopPersistenceModelTests()
    {
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=model_only;Password=model_only")
            .Options;
        using var dbContext = new MajlisDbContext(options, TimeProvider.System);
        _model = dbContext.GetService<IDesignTimeModel>().Model;
    }

    [Fact]
    public void Model_DefinesAttemptOwnershipUniquenessHistoryAndExactSnapshotChecks()
    {
        var attempt = _model.FindEntityType(typeof(UserAttempt));
        Assert.NotNull(attempt);

        Assert.Equal("UserAttempts", attempt.GetTableName());
        Assert.All(attempt.GetProperties(), property =>
            Assert.Equal(PropertySaveBehavior.Throw, property.GetAfterSaveBehavior()));
        Assert.Contains(attempt.GetIndexes(), index =>
            index.IsUnique &&
            index.GetDatabaseName() == "UX_UserAttempts_UserId_DailyMajlisId" &&
            Names(index.Properties).SequenceEqual([
                nameof(UserAttempt.UserId),
                nameof(UserAttempt.DailyMajlisId),
            ]));
        Assert.Contains(attempt.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_UserAttempts_UserId_AttemptedAt_Id" &&
            Names(index.Properties).SequenceEqual([
                nameof(UserAttempt.UserId),
                nameof(UserAttempt.AttemptedAt),
                nameof(UserAttempt.Id),
            ]) &&
            index.IsDescending is [false, true, true]);

        AssertForeignKey(
            attempt,
            "FK_UserAttempts_Challenges_ChallengeId_ContentRevisionId",
            [nameof(UserAttempt.ChallengeId), nameof(UserAttempt.ContentRevisionId)],
            ["Id", "RevisionId"],
            DeleteBehavior.Restrict);
        AssertForeignKey(
            attempt,
            "FK_UserAttempts_ChallengeOptions_SelectedOptionId_ChallengeId",
            [nameof(UserAttempt.SelectedOptionId), nameof(UserAttempt.ChallengeId)],
            ["Id", "ChallengeId"],
            DeleteBehavior.Restrict);

        var constraint = Assert.Single(
            attempt.GetCheckConstraints(),
            item => item.Name == "CK_UserAttempts_ExactXpAndSnapshots");
        Assert.Contains("\"CompletionXp\" = 10", constraint.Sql, StringComparison.Ordinal);
        Assert.Contains("\"CorrectnessXp\" IN (0, 5)", constraint.Sql, StringComparison.Ordinal);
        Assert.Contains("\"IsCorrect\" AND \"CorrectnessXp\" = 5", constraint.Sql, StringComparison.Ordinal);
        Assert.Contains("NOT \"IsCorrect\" AND \"CorrectnessXp\" = 0", constraint.Sql, StringComparison.Ordinal);
        Assert.Contains("\"LifetimeXpAfter\" >= 0", constraint.Sql, StringComparison.Ordinal);
        Assert.Contains("\"LongestStreakAfter\" >= \"CurrentStreakAfter\"", constraint.Sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Model_DefinesLedgerProgressAndIdempotencyConstraintsAndIndexes()
    {
        var ledger = _model.FindEntityType(typeof(XpLedgerEntry));
        var progress = _model.FindEntityType(typeof(UserProgress));
        var idempotency = _model.FindEntityType(typeof(IdempotencyRecord));
        Assert.NotNull(ledger);
        Assert.NotNull(progress);
        Assert.NotNull(idempotency);

        Assert.Equal("XpLedger", ledger.GetTableName());
        Assert.All(ledger.GetProperties(), property =>
            Assert.Equal(PropertySaveBehavior.Throw, property.GetAfterSaveBehavior()));
        Assert.Contains(ledger.GetIndexes(), index =>
            index.IsUnique &&
            index.GetDatabaseName() == "UX_XpLedger_AttemptId" &&
            Names(index.Properties).SequenceEqual([nameof(XpLedgerEntry.AttemptId)]));
        Assert.Contains(ledger.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_XpLedger_OccurredAt_Amount" &&
            Names(index.Properties).SequenceEqual([
                nameof(XpLedgerEntry.OccurredAt),
                nameof(XpLedgerEntry.Amount),
            ]));
        Assert.Contains(ledger.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_XpLedger_UserId_OccurredAt" &&
            Names(index.Properties).SequenceEqual([
                nameof(XpLedgerEntry.UserId),
                nameof(XpLedgerEntry.OccurredAt),
            ]));
        Assert.Single(
            ledger.GetCheckConstraints(),
            item => item.Name == "CK_XpLedger_ExactAmount" &&
                item.Sql.Contains("\"Amount\" IN (10, 15)", StringComparison.Ordinal));

        Assert.Equal("UserProgress", progress.GetTableName());
        Assert.Equal([nameof(UserProgress.UserId)], Names(progress.FindPrimaryKey()!.Properties));
        Assert.Single(
            progress.GetCheckConstraints(),
            item => item.Name == "CK_UserProgress_NonNegative" &&
                item.Sql.Contains("\"LongestStreak\" >= \"CurrentStreak\"", StringComparison.Ordinal));

        Assert.Equal("IdempotencyRecords", idempotency.GetTableName());
        Assert.All(idempotency.GetProperties(), property =>
            Assert.Equal(PropertySaveBehavior.Throw, property.GetAfterSaveBehavior()));
        Assert.Equal(
            [
                nameof(IdempotencyRecord.UserId),
                nameof(IdempotencyRecord.Scope),
                nameof(IdempotencyRecord.IdempotencyKey),
            ],
            Names(idempotency.FindPrimaryKey()!.Properties));
        Assert.Contains(idempotency.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_IdempotencyRecords_ExpiresAt" &&
            Names(index.Properties).SequenceEqual([nameof(IdempotencyRecord.ExpiresAt)]));
    }

    [Fact]
    public void Model_DefinesImmutableUniquePublicationHistory()
    {
        var publication = _model.FindEntityType(typeof(DailyMajlisPublication));
        Assert.NotNull(publication);

        Assert.Equal("DailyMajlisPublications", publication.GetTableName());
        Assert.All(publication.GetProperties(), property =>
            Assert.Equal(PropertySaveBehavior.Throw, property.GetAfterSaveBehavior()));
        Assert.Contains(publication.GetIndexes(), index =>
            index.IsUnique &&
            Names(index.Properties).SequenceEqual([nameof(DailyMajlisPublication.PublishDate)]));
        Assert.Contains(publication.GetForeignKeys(), foreignKey =>
            foreignKey.DeleteBehavior == DeleteBehavior.Restrict &&
            Names(foreignKey.Properties).SequenceEqual([
                nameof(DailyMajlisPublication.DailyMajlisId),
            ]));
    }

    private static string[] Names(IEnumerable<IReadOnlyProperty> properties) =>
        properties.Select(property => property.Name).ToArray();

    private static void AssertForeignKey(
        IReadOnlyEntityType entityType,
        string constraintName,
        string[] dependentProperties,
        string[] principalProperties,
        DeleteBehavior deleteBehavior)
    {
        Assert.Contains(entityType.GetForeignKeys(), foreignKey =>
            foreignKey.GetConstraintName() == constraintName &&
            Names(foreignKey.Properties).SequenceEqual(dependentProperties) &&
            Names(foreignKey.PrincipalKey.Properties).SequenceEqual(principalProperties) &&
            foreignKey.DeleteBehavior == deleteBehavior);
    }
}
