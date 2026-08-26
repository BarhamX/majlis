using Majlis.Domain.Identity;

namespace Majlis.Infrastructure.Persistence;

internal static class IdentityStorage
{
    public static string ToStorage(ExternalIdentityProvider value) => value switch
    {
        ExternalIdentityProvider.Google => "google",
        ExternalIdentityProvider.Apple => "apple",
        ExternalIdentityProvider.Meta => "meta",
        ExternalIdentityProvider.Snapchat => "snapchat",
        ExternalIdentityProvider.Test => "test",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static ExternalIdentityProvider ToExternalIdentityProvider(string value) => value switch
    {
        "google" => ExternalIdentityProvider.Google,
        "apple" => ExternalIdentityProvider.Apple,
        "meta" => ExternalIdentityProvider.Meta,
        "snapchat" => ExternalIdentityProvider.Snapchat,
        "test" => ExternalIdentityProvider.Test,
        _ => throw new InvalidOperationException($"Unknown external identity provider '{value}'."),
    };

    public static string ToStorage(UserAccountStatus value) => value switch
    {
        UserAccountStatus.Active => "active",
        UserAccountStatus.Suspended => "suspended",
        UserAccountStatus.DeletionPending => "deletion_pending",
        UserAccountStatus.Deleted => "deleted",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static UserAccountStatus ToUserAccountStatus(string value) => value switch
    {
        "active" => UserAccountStatus.Active,
        "suspended" => UserAccountStatus.Suspended,
        "deletion_pending" => UserAccountStatus.DeletionPending,
        "deleted" => UserAccountStatus.Deleted,
        _ => throw new InvalidOperationException($"Unknown user account status '{value}'."),
    };

    public static string ToStorage(AgeBand value) => value switch
    {
        AgeBand.Minor13To17 => "13_17",
        AgeBand.Adult18Plus => "18_plus",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static AgeBand ToAgeBand(string value) => value switch
    {
        "13_17" => AgeBand.Minor13To17,
        "18_plus" => AgeBand.Adult18Plus,
        _ => throw new InvalidOperationException($"Unknown age band '{value}'."),
    };

    public static string ToStorage(LeaderboardVisibility value) => value switch
    {
        LeaderboardVisibility.Private => "private",
        LeaderboardVisibility.GlobalWeekly => "global_weekly",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static LeaderboardVisibility ToLeaderboardVisibility(string value) => value switch
    {
        "private" => LeaderboardVisibility.Private,
        "global_weekly" => LeaderboardVisibility.GlobalWeekly,
        _ => throw new InvalidOperationException($"Unknown leaderboard visibility '{value}'."),
    };

    public static string ToStorage(ConsentType value) => value switch
    {
        ConsentType.Terms => "terms",
        ConsentType.Privacy => "privacy",
        ConsentType.Analytics => "analytics",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static ConsentType ToConsentType(string value) => value switch
    {
        "terms" => ConsentType.Terms,
        "privacy" => ConsentType.Privacy,
        "analytics" => ConsentType.Analytics,
        _ => throw new InvalidOperationException($"Unknown consent type '{value}'."),
    };

    public static string ToStorage(AccountDeletionStatus value) => value switch
    {
        AccountDeletionStatus.Requested => "requested",
        AccountDeletionStatus.IdentityDeleted => "identity_deleted",
        AccountDeletionStatus.ActiveDataPurged => "active_data_purged",
        AccountDeletionStatus.BackupExpiryPending => "backup_expiry_pending",
        AccountDeletionStatus.Completed => "completed",
        AccountDeletionStatus.LegalHold => "legal_hold",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static AccountDeletionStatus ToAccountDeletionStatus(string value) => value switch
    {
        "requested" => AccountDeletionStatus.Requested,
        "identity_deleted" => AccountDeletionStatus.IdentityDeleted,
        "active_data_purged" => AccountDeletionStatus.ActiveDataPurged,
        "backup_expiry_pending" => AccountDeletionStatus.BackupExpiryPending,
        "completed" => AccountDeletionStatus.Completed,
        "legal_hold" => AccountDeletionStatus.LegalHold,
        _ => throw new InvalidOperationException($"Unknown deletion status '{value}'."),
    };

    public static string ToStorage(UserRole value) => value switch
    {
        UserRole.Moderator => "moderator",
        UserRole.ContentEditor => "content_editor",
        UserRole.ContentReviewer => "content_reviewer",
        UserRole.Publisher => "publisher",
        UserRole.OperationsAdmin => "operations_admin",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public static UserRole ToUserRole(string value) => value switch
    {
        "moderator" => UserRole.Moderator,
        "content_editor" => UserRole.ContentEditor,
        "content_reviewer" => UserRole.ContentReviewer,
        "publisher" => UserRole.Publisher,
        "operations_admin" => UserRole.OperationsAdmin,
        _ => throw new InvalidOperationException($"Unknown user role '{value}'."),
    };
}
