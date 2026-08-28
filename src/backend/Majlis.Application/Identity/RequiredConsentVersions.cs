namespace Majlis.Application.Identity;

public sealed record RequiredConsentVersions
{
    public RequiredConsentVersions(string terms, string privacy)
    {
        Terms = RequireVersion(terms, nameof(terms));
        Privacy = RequireVersion(privacy, nameof(privacy));
    }

    public string Terms { get; }

    public string Privacy { get; }

    private static string RequireVersion(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A current consent version is required.", parameterName)
            : value.Trim();
}
