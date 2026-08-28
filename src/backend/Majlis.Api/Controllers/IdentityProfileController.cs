using Majlis.Api.Authentication;
using Majlis.Application.Identity;
using Majlis.Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Majlis.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class IdentityProfileController(
    IIdentityProfileService identityProfileService) : ControllerBase
{
    [HttpPost("bootstrap")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserProfileResponse>> Bootstrap(
        BootstrapProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await identityProfileService.BootstrapAsync(
                AuthenticatedIdentityFactory.Create(User),
                request,
                cancellationToken);
            return result.Created
                ? StatusCode(StatusCodes.Status201Created, result.Profile)
                : Ok(result.Profile);
        }
        catch (IdentityProfileException exception)
        {
            return ToProblem(exception);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception);
        }
    }

    [HttpGet("profile")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await identityProfileService.GetProfileAsync(
                AuthenticatedIdentityFactory.Create(User),
                cancellationToken));
        }
        catch (IdentityProfileException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPut("profile")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await identityProfileService.UpdateProfileAsync(
                AuthenticatedIdentityFactory.Create(User),
                request,
                cancellationToken));
        }
        catch (IdentityProfileException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPost("sessions/revoke-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        try
        {
            await identityProfileService.RevokeAllSessionsAsync(
                AuthenticatedIdentityFactory.Create(User),
                cancellationToken);
            return NoContent();
        }
        catch (IdentityProfileException exception)
        {
            return ToProblem(exception);
        }
    }

    [HttpPost("deletion-requests")]
    [ProducesResponseType<AccountDeletionResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AccountDeletionResponse>> RequestDeletion(
        AccountDeletionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await identityProfileService.RequestDeletionAsync(
                AuthenticatedIdentityFactory.Create(User),
                request,
                cancellationToken);
            return Accepted(response);
        }
        catch (IdentityProfileException exception)
        {
            return ToProblem(exception);
        }
    }

    private ObjectResult ToProblem(IdentityProfileException exception) => Problem(
        statusCode: exception.Code switch
        {
            "authentication_required" => StatusCodes.Status401Unauthorized,
            "profile_incomplete" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status422UnprocessableEntity,
        },
        title: exception.Message,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = exception.Code,
        });

    private ObjectResult ValidationProblem(ArgumentException exception) => Problem(
        statusCode: StatusCodes.Status422UnprocessableEntity,
        title: exception.Message,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = "validation_failed",
        });
}
