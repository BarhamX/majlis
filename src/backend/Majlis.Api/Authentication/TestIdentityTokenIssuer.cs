using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Globalization;
using Majlis.Contracts.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Majlis.Api.Authentication;

public interface ITestIdentityTokenIssuer
{
    TestAccessTokenResponse Issue(string subject);
}

internal sealed class TestIdentityTokenIssuer(
    TestIdentitySettings settings) : ITestIdentityTokenIssuer
{
    public TestAccessTokenResponse Issue(string subject)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddHours(1);
        var credentials = new SigningCredentials(
            settings.SigningKey,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, subject),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer64),
                new Claim("majlis_provider", "test"),
                new Claim("majlis_issuer", settings.Issuer),
            ],
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new TestAccessTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAt);
    }
}
