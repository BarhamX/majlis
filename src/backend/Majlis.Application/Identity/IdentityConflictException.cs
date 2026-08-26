namespace Majlis.Application.Identity;

public sealed class IdentityConflictException : Exception
{
    public IdentityConflictException(Exception innerException)
        : base("The external identity is already linked.", innerException)
    {
    }
}
