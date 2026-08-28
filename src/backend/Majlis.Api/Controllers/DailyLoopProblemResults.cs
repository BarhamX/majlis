using Majlis.Application.DailyLoop;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

internal static class DailyLoopProblemResults
{
    public static ObjectResult Create(DailyLoopException exception)
    {
        var status = exception.Code switch
        {
            "idempotency_key_reused" or "attempt_already_completed" =>
                StatusCodes.Status409Conflict,
            "option_not_in_challenge" or "validation_failed" =>
                StatusCodes.Status422UnprocessableEntity,
            "daily_majlis_unavailable" or "attempt_not_found" =>
                StatusCodes.Status404NotFound,
            "profile_incomplete" or "forbidden" =>
                StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status422UnprocessableEntity,
        };
        return Create(status, exception.Code, exception.Message, exception.AttemptId);
    }

    public static ObjectResult AttemptNotFound() => Create(
        StatusCodes.Status404NotFound,
        "attempt_not_found",
        "The attempt was not found.");

    public static ObjectResult Create(
        int status,
        string code,
        string title,
        Guid? attemptId = null)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
        };
        problem.Extensions["code"] = code;
        if (attemptId.HasValue)
        {
            problem.Extensions["attemptId"] = attemptId.Value;
        }

        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };
    }
}
