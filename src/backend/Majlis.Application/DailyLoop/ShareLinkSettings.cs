namespace Majlis.Application.DailyLoop;

public sealed record ShareLinkSettings
{
    public ShareLinkSettings(string publicHost)
    {
        if (!Uri.TryCreate(publicHost, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new InvalidOperationException(
                "Sharing:PublicHost must be an absolute HTTPS origin.");
        }

        PublicHost = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    public string PublicHost { get; }
}
