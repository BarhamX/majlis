using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Majlis.Infrastructure.Persistence;

internal static class DailyMajlisInitializationConflict
{
    private const string DailyMajlisPublishDateConstraint =
        "IX_DailyMajlis_PublishDate";
    private const string PublicationPublishDateConstraint =
        "IX_DailyMajlisPublications_PublishDate";
    private const string RevisionNumberConstraint =
        "IX_DailyMajlisRevisions_DailyMajlisId_RevisionNumber";

    public static bool IsExpectedCreateRace(DbUpdateException exception) =>
        HasUniqueConstraint(
            exception,
            DailyMajlisPublishDateConstraint,
            PublicationPublishDateConstraint);

    public static bool IsExpectedRepairRace(DbUpdateException exception) =>
        HasUniqueConstraint(exception, RevisionNumberConstraint);

    private static bool HasUniqueConstraint(
        DbUpdateException exception,
        params string[] expectedConstraints) => exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
        } postgresException && expectedConstraints.Contains(
            postgresException.ConstraintName,
            StringComparer.Ordinal);
}
