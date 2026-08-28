using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Majlis.Tests.Infrastructure;

public sealed class DailyMajlisInitializationConflictTests
{
    [Theory]
    [InlineData("PK_DailyMajlis")]
    [InlineData("IX_DailyMajlis_PublishDate")]
    [InlineData("IX_DailyMajlisPublications_PublishDate")]
    public void IsExpectedCreateRace_OnlyRecognizesDeterministicSeedConstraints(
        string constraintName)
    {
        Assert.True(DailyMajlisInitializationConflict.IsExpectedCreateRace(
            CreateUniqueViolation(constraintName)));
    }

    [Theory]
    [InlineData("PK_DailyMajlisPublications")]
    [InlineData("IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber")]
    [InlineData("UX_Unrelated_Test_Constraint")]
    public void IsExpectedCreateRace_RepairAndUnrelatedConstraints_AreNotConvergence(
        string constraintName)
    {
        Assert.False(DailyMajlisInitializationConflict.IsExpectedCreateRace(
            CreateUniqueViolation(constraintName)));
    }

    [Theory]
    [InlineData("IX_DailyMajlis_PublishDate")]
    [InlineData("IX_DailyMajlisPublications_PublishDate")]
    public void IsExpectedEditorialWinnerRace_OnlyRecognizesDateOwnershipConstraints(
        string constraintName)
    {
        Assert.True(DailyMajlisInitializationConflict.IsExpectedEditorialWinnerRace(
            CreateUniqueViolation(constraintName)));
    }

    [Theory]
    [InlineData("PK_DailyMajlis")]
    [InlineData("PK_DailyMajlisPublications")]
    [InlineData("IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber")]
    [InlineData("UX_Unrelated_Test_Constraint")]
    public void IsExpectedEditorialWinnerRace_OtherConstraintsAreNotConvergence(
        string constraintName)
    {
        Assert.False(DailyMajlisInitializationConflict.IsExpectedEditorialWinnerRace(
            CreateUniqueViolation(constraintName)));
    }

    [Theory]
    [InlineData("IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber", false)]
    [InlineData("IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber", true)]
    [InlineData("PK_DailyMajlisPublications", true)]
    public void IsExpectedRepairRace_RecognizesOnlyRepairConvergenceConstraints(
        string constraintName,
        bool publicationWasMissing)
    {
        Assert.True(DailyMajlisInitializationConflict.IsExpectedRepairRace(
            CreateUniqueViolation(constraintName),
            publicationWasMissing));
    }

    [Theory]
    [InlineData("PK_DailyMajlisPublications", false)]
    [InlineData("IX_DailyMajlis_PublishDate", true)]
    [InlineData("IX_DailyMajlisPublications_PublishDate", true)]
    [InlineData("UX_Unrelated_Test_Constraint", true)]
    public void IsExpectedRepairRace_CreateAndUnrelatedConstraints_AreNotConvergence(
        string constraintName,
        bool publicationWasMissing)
    {
        Assert.False(DailyMajlisInitializationConflict.IsExpectedRepairRace(
            CreateUniqueViolation(constraintName),
            publicationWasMissing));
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
