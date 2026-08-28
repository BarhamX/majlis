namespace Majlis.Domain.Identity;

public sealed class AccountDeletionRequest
{
    private AccountDeletionRequest()
    {
    }

    internal AccountDeletionRequest(Guid id, Guid userId, DateTimeOffset requestedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A deletion request id is required.", nameof(id));
        }

        Id = id;
        UserId = userId;
        Status = AccountDeletionStatus.Requested;
        RequestedAt = requestedAt;
        PurgeDueAt = requestedAt.AddDays(30);
        BackupExpiryDueAt = requestedAt.AddDays(65);
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public AccountDeletionStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset PurgeDueAt { get; private set; }

    public DateTimeOffset BackupExpiryDueAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? LegalHoldReason { get; private set; }
}
