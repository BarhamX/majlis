using Majlis.Api.Authentication;
using Majlis.Application.DailyLoop;
using Majlis.Contracts.DailyLoop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

[ApiController]
[Authorize(Policy = MajlisAuthorizationPolicies.CompletedProfile)]
[Route("api/v1/me")]
public sealed class MeDailyLoopController(IDailyLoopService dailyLoopService) : ControllerBase
{
    [HttpGet("attempts")]
    [ProducesResponseType<AttemptHistoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AttemptHistoryResponse>> GetAttempts(
        [FromQuery] string? cursor,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await dailyLoopService.GetAttemptHistoryAsync(
                AuthenticatedIdentityFactory.Create(User),
                cursor,
                limit,
                cancellationToken));
        }
        catch (DailyLoopException exception)
        {
            return DailyLoopProblemResults.Create(exception, HttpContext);
        }
    }

    [HttpGet("progress")]
    [ProducesResponseType<UserProgressResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProgressResponse>> GetProgress(
        CancellationToken cancellationToken) => Ok(
        await dailyLoopService.GetProgressAsync(
            AuthenticatedIdentityFactory.Create(User),
            cancellationToken));
}
