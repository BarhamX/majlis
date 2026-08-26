using System.IdentityModel.Tokens.Jwt;
using Majlis.Api.Authentication;
using Majlis.Domain.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Majlis.Tests.Infrastructure;

public sealed class TestIdentityTokenIssuerTests
{
    [Fact]
    public void Issue_CreatesAValidatedTestIdentity()
    {
        var settings = new TestIdentitySettings(
            "https://test.majlis.local",
            "majlis-api",
            new SymmetricSecurityKey(new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 }));
        var issuer = new TestIdentityTokenIssuer(settings);

        var response = issuer.Issue("local-user");
        var tokenHandler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false,
        };
        var principal = tokenHandler.ValidateToken(
            response.AccessToken,
            new TokenValidationParameters
            {
                ValidIssuer = settings.Issuer,
                ValidAudience = settings.Audience,
                IssuerSigningKey = settings.SigningKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = JwtRegisteredClaimNames.Sub,
            },
            out _);

        var identity = AuthenticatedIdentityFactory.Create(principal);

        Assert.Equal(ExternalIdentityProvider.Test, identity.Provider);
        Assert.Equal(settings.Issuer, identity.Issuer);
        Assert.Equal("local-user", identity.Subject);
    }
}
