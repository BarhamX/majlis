using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Majlis.Api.Authentication;

internal static class MajlisAuthenticationExtensions
{
    private const string TestMode = "Test";

    public static IServiceCollection AddMajlisAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var mode = configuration["Authentication:Mode"];
        if (!string.Equals(mode, TestMode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Production identity adapters are deferred until Game Ready. " +
                "Set Authentication:Mode to Test only in Development or Testing.");
        }

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            throw new InvalidOperationException(
                "Test authentication is allowed only in Development and Testing environments.");
        }

        var settings = new TestIdentitySettings(
            "https://test.majlis.local",
            "majlis-api",
            new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)));
        services.AddSingleton(settings);
        services.AddSingleton<ITestIdentityTokenIssuer, TestIdentityTokenIssuer>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = settings.SigningKey,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5),
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        try
                        {
                            var identity = AuthenticatedIdentityFactory.Create(context.Principal!);
                            if (identity.Provider != Majlis.Domain.Identity.ExternalIdentityProvider.Test)
                            {
                                context.Fail("Only the test identity provider is valid in Test mode.");
                            }
                        }
                        catch (Exception exception) when (
                            exception is InvalidOperationException or FormatException or ArgumentOutOfRangeException)
                        {
                            context.Fail("The authenticated identity claims are invalid.");
                        }

                        return Task.CompletedTask;
                    },
                };
            });
        services.AddScoped<IAuthorizationHandler, CompletedProfileAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, MajlisAuthorizationResultHandler>();
        services.AddAuthorization(options => options.AddPolicy(
            MajlisAuthorizationPolicies.CompletedProfile,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new CompletedProfileRequirement());
            }));

        return services;
    }
}

internal sealed record TestIdentitySettings(
    string Issuer,
    string Audience,
    SymmetricSecurityKey SigningKey);
