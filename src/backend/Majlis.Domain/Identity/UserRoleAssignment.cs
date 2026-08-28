namespace Majlis.Domain.Identity;

public sealed class UserRoleAssignment
{
    private UserRoleAssignment()
    {
    }

    public UserRoleAssignment(
        Guid id,
        Guid userId,
        UserRole role,
        Guid assignedByUserId,
        DateTimeOffset assignedAt)
    {
        if (id == Guid.Empty || userId == Guid.Empty || assignedByUserId == Guid.Empty)
        {
            throw new ArgumentException("Role assignment ids are required.");
        }

        Id = id;
        UserId = userId;
        Role = role;
        AssignedByUserId = assignedByUserId;
        AssignedAt = assignedAt;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public UserRole Role { get; private set; }

    public Guid AssignedByUserId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }
}
