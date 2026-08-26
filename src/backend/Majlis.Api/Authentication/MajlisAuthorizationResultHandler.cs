using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Majlis.Api.Authentication;

internal sealed class MajlisAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden &&
            policy.Requirements.OfType<CompletedProfileRequirement>().Any())
        {
            await Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Complete the Majlis profile before continuing.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "profile_incomplete",
                }).ExecuteAsync(context);
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
