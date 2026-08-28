namespace Majlis.Application.Identity;

public sealed class IdentityProfileException(
    string code,
    string message) : Exception(message)
{
    public string Code { get; } = code;
}
