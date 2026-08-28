using Majlis.Application.DailyMajlis;
using Majlis.Api.Authentication;
using Majlis.Contracts.DailyMajlis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

[ApiController]
[Authorize(Policy = MajlisAuthorizationPolicies.CompletedProfile)]
[Route("api/v1/daily-majlis")]
public sealed class DailyMajlisController(IDailyMajlisService dailyMajlisService) : ControllerBase
{
    [HttpGet("today")]
    [ProducesResponseType<DailyMajlisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyMajlisResponse>> GetToday(
        CancellationToken cancellationToken)
    {
        Response.Headers.Vary = "Accept-Language";
        var localized = await dailyMajlisService.GetTodayAsync(
            Request.Headers.AcceptLanguage.ToString(),
            cancellationToken);

        if (localized is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Today's Majlis is not available yet.",
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "daily_majlis_unavailable",
                });
        }

        Response.Headers.ContentLanguage = localized.ContentLanguage;
        Response.Headers.Vary = "Accept-Language";
        return Ok(localized.Response);
    }
}
