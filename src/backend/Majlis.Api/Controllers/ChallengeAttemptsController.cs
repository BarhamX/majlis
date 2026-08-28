using Majlis.Api.Authentication;
using Majlis.Application.DailyLoop;
using Majlis.Contracts.DailyLoop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

[ApiController]
[Authorize(Policy = MajlisAuthorizationPolicies.CompletedProfile)]
[Route("api/v1/challenges/{challengeId:guid}/attempts")]
public sealed class ChallengeAttemptsController(IDailyLoopService dailyLoopService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AttemptResultResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<AttemptResultResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AttemptResultResponse>> Submit(
        Guid challengeId,
        SubmitAttemptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(idempotencyKey, out var parsedKey) || parsedKey == Guid.Empty)
        {
            return DailyLoopProblemResults.Create(
                StatusCodes.Status422UnprocessableEntity,
                "validation_failed",
                "Idempotency-Key must be a non-empty UUID.",
                HttpContext);
        }

        try
        {
            var result = await dailyLoopService.SubmitAttemptAsync(
                AuthenticatedIdentityFactory.Create(User),
                challengeId,
                request.SelectedOptionId,
                parsedKey,
                Request.Headers.AcceptLanguage.ToString(),
                cancellationToken);
            Response.Headers.ContentLanguage = result.Response.ResultLocale;
            return result.IsReplay
                ? Ok(result.Response)
                : StatusCode(StatusCodes.Status201Created, result.Response);
        }
        catch (DailyLoopException exception)
        {
            return DailyLoopProblemResults.Create(exception, HttpContext);
        }
        catch (ArgumentException exception)
        {
            return DailyLoopProblemResults.Create(
                StatusCodes.Status422UnprocessableEntity,
                "validation_failed",
                exception.Message,
                HttpContext);
        }
    }
}
