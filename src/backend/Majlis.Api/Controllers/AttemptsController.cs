using Majlis.Api.Authentication;
using Majlis.Application.DailyLoop;
using Majlis.Contracts.DailyLoop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

[ApiController]
[Authorize(Policy = MajlisAuthorizationPolicies.CompletedProfile)]
[Route("api/v1/attempts")]
public sealed class AttemptsController(IDailyLoopService dailyLoopService) : ControllerBase
{
    [HttpGet("{attemptId:guid}")]
    [ProducesResponseType<AttemptResultResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttemptResultResponse>> Get(
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var result = await dailyLoopService.GetAttemptAsync(
            AuthenticatedIdentityFactory.Create(User),
            attemptId,
            cancellationToken);
        if (result is null)
        {
            return DailyLoopProblemResults.AttemptNotFound(HttpContext);
        }

        Response.Headers.ContentLanguage = result.ResultLocale;
        return Ok(result);
    }

    [HttpGet("{attemptId:guid}/share")]
    [ProducesResponseType<AttemptShareResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttemptShareResponse>> GetShare(
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var result = await dailyLoopService.GetShareAsync(
            AuthenticatedIdentityFactory.Create(User),
            attemptId,
            cancellationToken);
        return result is null
            ? DailyLoopProblemResults.AttemptNotFound(HttpContext)
            : Ok(result);
    }
}
