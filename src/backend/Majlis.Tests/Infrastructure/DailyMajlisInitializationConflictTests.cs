using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Majlis.Tests.Infrastructure;

public sealed class DailyMajlisInitializationConflictTests
{
    [Theory]
    [InlineData("IX_DailyMajlis_PublishDate")]
    [InlineData("IX_DailyMajlisPublications_PublishDate")]
    public void IsExpectedCreateRace_OnlyRecognizesPublishDateConstraints(
        string constraintName)
    {
        Assert.True(DailyMajlisInitializationConflict.IsExpectedCreateRace(
            CreateUniqueViolation(constraintName)));
    }

    [Fact]
    public void IsExpectedCreateRace_UnrelatedUniqueConstraint_IsNotConvergence()
    {
        Assert.False(DailyMajlisInitializationConflict.IsExpectedCreateRace(
            CreateUniqueViolation("UX_Unrelated_Test_Constraint")));
    }

    [Fact]
    public void IsExpectedRepairRace_OnlyRecognizesRevisionNumberConstraint()
    {
        Assert.True(DailyMajlisInitializationConflict.IsExpectedRepairRace(
            CreateUniqueViolation("IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber")));
        Assert.False(DailyMajlisInitializationConflict.IsExpectedRepairRace(
            CreateUniqueViolation("IX_DailyMajlisPublications_PublishDate")));
    }

    private static DbUpdateException CreateUniqueViolation(string constraintName) => new(
        "Test unique violation.",
        new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation,
            constraintName: constraintName));
}
