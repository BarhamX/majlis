using System.Text.RegularExpressions;
using Majlis.Api.Authentication;
using Majlis.Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

[ApiController]
[Route("api/v1/dev/auth")]
public sealed partial class DevelopmentAuthenticationController(
    IHostEnvironment environment,
    IServiceProvider serviceProvider) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType<TestAccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<TestAccessTokenResponse> Issue(TestAccessTokenRequest request)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            return NotFound();
        }

        var subject = request.Subject?.Trim();
        if (string.IsNullOrEmpty(subject) || !SubjectPattern().IsMatch(subject))
        {
            return Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Test subject is invalid.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "validation_failed",
                });
        }

        var tokenIssuer = serviceProvider.GetRequiredService<ITestIdentityTokenIssuer>();
        return Ok(tokenIssuer.Issue(subject));
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,100}$", RegexOptions.CultureInvariant)]
    private static partial Regex SubjectPattern();
}
