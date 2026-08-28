using Majlis.Contracts.Identity;

namespace Majlis.Application.Identity;

public interface IIdentityProfileService
{
    Task<(UserProfileResponse Profile, bool Created)> BootstrapAsync(
        AuthenticatedIdentity identity,
        BootstrapProfileRequest request,
        CancellationToken cancellationToken);

    Task<UserProfileResponse> GetProfileAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken);

    Task<UserProfileResponse> UpdateProfileAsync(
        AuthenticatedIdentity identity,
        UpdateProfileRequest request,
        CancellationToken cancellationToken);

    Task RevokeAllSessionsAsync(
        AuthenticatedIdentity identity,
        CancellationToken cancellationToken);

    Task<AccountDeletionResponse> RequestDeletionAsync(
        AuthenticatedIdentity identity,
        AccountDeletionRequest request,
        CancellationToken cancellationToken);
}
