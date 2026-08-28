namespace Majlis.Domain.Identity;

public enum ExternalIdentityProvider
{
    Google,
    Apple,
    Meta,
    Snapchat,
    Test,
}

public enum UserAccountStatus
{
    Active,
    Suspended,
    DeletionPending,
    Deleted,
}

public enum AgeBand
{
    Minor13To17,
    Adult18Plus,
}

public enum LeaderboardVisibility
{
    Private,
    GlobalWeekly,
}

public enum ConsentType
{
    Terms,
    Privacy,
    Analytics,
}

public enum AccountDeletionStatus
{
    Requested,
    IdentityDeleted,
    ActiveDataPurged,
    BackupExpiryPending,
    Completed,
    LegalHold,
}

public enum UserRole
{
    Moderator,
    ContentEditor,
    ContentReviewer,
    Publisher,
    OperationsAdmin,
}
