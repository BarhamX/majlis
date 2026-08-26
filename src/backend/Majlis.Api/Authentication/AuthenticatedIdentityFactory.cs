using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Majlis.Application.Identity;
using Majlis.Domain.Identity;

namespace Majlis.Api.Authentication;

internal static class AuthenticatedIdentityFactory
{
    public static AuthenticatedIdentity Create(ClaimsPrincipal principal)
    {
        var provider = ParseProvider(RequiredClaim(principal, "majlis_provider"));
        var issuer = RequiredClaim(principal, "majlis_issuer");
        var subject = RequiredClaim(principal, JwtRegisteredClaimNames.Sub);
        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(
            RequiredClaim(principal, JwtRegisteredClaimNames.Iat),
            CultureInfo.InvariantCulture));

        return new AuthenticatedIdentity(provider, issuer, subject, issuedAt);
    }

    private static string RequiredClaim(ClaimsPrincipal principal, string type) =>
        principal.FindFirstValue(type) ?? throw new InvalidOperationException(
            $"Authenticated identity is missing the required '{type}' claim.");

    private static ExternalIdentityProvider ParseProvider(string value) => value switch
    {
        "google" => ExternalIdentityProvider.Google,
        "apple" => ExternalIdentityProvider.Apple,
        "meta" => ExternalIdentityProvider.Meta,
        "snapchat" => ExternalIdentityProvider.Snapchat,
        "test" => ExternalIdentityProvider.Test,
        _ => throw new InvalidOperationException("Authenticated identity provider is not supported."),
    };
}
