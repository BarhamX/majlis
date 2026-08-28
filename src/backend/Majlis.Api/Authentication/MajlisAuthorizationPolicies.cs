using Majlis.Application.Identity;
using Majlis.Api.RateLimiting;
using Majlis.Domain.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Majlis.Api.Authentication;

public static class MajlisAuthorizationPolicies
{
    public const string CompletedProfile = "completed_profile";
}

internal sealed class CompletedProfileRequirement : IAuthorizationRequirement;

internal sealed class CompletedProfileAuthorizationHandler(
    IUserAccountRepository userAccountRepository) : AuthorizationHandler<CompletedProfileRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompletedProfileRequirement requirement)
    {
        try
        {
            var identity = AuthenticatedIdentityFactory.Create(context.User);
            var cancellationToken = context.Resource is HttpContext requestContext
                ? requestContext.RequestAborted
                : CancellationToken.None;
            var user = await userAccountRepository.FindByIdentityAsync(
                identity.Provider,
                identity.Issuer,
                identity.Subject,
                cancellationToken);

            if (user is not null &&
                user.Status == UserAccountStatus.Active &&
                user.Profile is not null &&
                (!user.AuthenticationNotBefore.HasValue ||
                 identity.IssuedAt > user.AuthenticationNotBefore.Value))
            {
                if (context.Resource is HttpContext httpContext)
                {
                    httpContext.Items[DailyAttemptRateLimiting.AccountIdItemKey] = user.Id;
                }

                context.Succeed(requirement);
            }
        }
        catch (InvalidOperationException)
        {
            // Invalid or incomplete normalized identity claims fail closed.
        }
    }
}
