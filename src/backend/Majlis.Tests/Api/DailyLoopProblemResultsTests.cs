using Majlis.Api.Controllers;
using Majlis.Application.DailyLoop;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Tests.Api;

public sealed class DailyLoopProblemResultsTests
{
    [Theory]
    [InlineData("daily_majlis_unavailable", StatusCodes.Status404NotFound)]
    [InlineData("attempt_already_completed", StatusCodes.Status409Conflict)]
    [InlineData("option_not_in_challenge", StatusCodes.Status422UnprocessableEntity)]
    public void Create_RepresentativeDailyLoopFailure_ReturnsStableProblemContract(
        string code,
        int expectedStatus)
    {
        var result = DailyLoopProblemResults.Create(
            new DailyLoopException(code, "Safe public title."),
            new DefaultHttpContext { TraceIdentifier = "trace-daily-loop-contract" });

        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(expectedStatus, result.StatusCode);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal("Safe public title.", problem.Title);
        Assert.Equal("https://httpstatuses.com/" + expectedStatus, problem.Type);
        Assert.Equal(code, problem.Extensions["code"]);
        Assert.Equal("trace-daily-loop-contract", problem.Extensions["traceId"]);
        Assert.Null(problem.Detail);
    }
}
