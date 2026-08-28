using Majlis.Application.Identity;
using Majlis.Contracts.Identity;
using Majlis.Domain.Identity;
using AccountDeletionContract = Majlis.Contracts.Identity.AccountDeletionRequest;

namespace Majlis.Tests.Application;

public sealed class IdentityProfileServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Bootstrap_WhenIdentityIsNew_CreatesPrivateProfileAndRequiredConsents()
    {
        var repository = new FakeUserAccountRepository();
        var service = CreateService(repository);

        var result = await service.BootstrapAsync(
            TestIdentity("new-user"),
            BootstrapRequest("مريم", "18_plus"),
            CancellationToken.None);

        Assert.True(result.Created);
        Assert.Equal("private", result.Profile.LeaderboardVisibility);
        Assert.Equal(["test"], result.Profile.LinkedProviders);
        Assert.Equal(2, Assert.Single(repository.Users).Consents.Count);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Bootstrap_WhenIdentityAlreadyExists_ReturnsSameUser()
    {
        var repository = new FakeUserAccountRepository();
        var service = CreateService(repository);
        var identity = TestIdentity("same-user");

        var first = await service.BootstrapAsync(
            identity,
            BootstrapRequest("مريم", "18_plus"),
            CancellationToken.None);
        var second = await service.BootstrapAsync(
            identity,
            BootstrapRequest("Different Name", "18_plus"),
            CancellationToken.None);

        Assert.True(first.Created);
        Assert.False(second.Created);
        Assert.Equal(first.Profile.UserId, second.Profile.UserId);
        Assert.Equal("مريم", second.Profile.DisplayName);
        Assert.Single(repository.Users);
    }

    [Fact]
    public async Task Bootstrap_WhenUnderThirteen_RejectsWithoutPersistence()
    {
        var repository = new FakeUserAccountRepository();
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<IdentityProfileException>(() =>
            service.BootstrapAsync(
                TestIdentity("underage"),
                BootstrapRequest("Young User", "under_13"),
                CancellationToken.None));

        Assert.Equal("age_not_eligible", exception.Code);
        Assert.Empty(repository.Users);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Bootstrap_WhenConsentVersionIsNotCurrent_RejectsWithoutPersistence()
    {
        var repository = new FakeUserAccountRepository();
        var service = CreateService(repository);
        var request = BootstrapRequest("Consent User", "18_plus") with
        {
            AcceptedTermsVersion = "fabricated",
        };

        var exception = await Assert.ThrowsAsync<IdentityProfileException>(() =>
            service.BootstrapAsync(
                TestIdentity("invalid-consent"),
                request,
                CancellationToken.None));

        Assert.Equal("validation_failed", exception.Code);
        Assert.Empty(repository.Users);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task RequestDeletion_RevokesTheCurrentCredential()
    {
        var repository = new FakeUserAccountRepository();
        var service = CreateService(repository);
        var identity = TestIdentity("delete-user");
        await service.BootstrapAsync(
            identity,
            BootstrapRequest("Delete Me", "18_plus"),
            CancellationToken.None);

        var deletion = await service.RequestDeletionAsync(
            identity,
            new AccountDeletionContract("delete_my_account"),
            CancellationToken.None);
        var exception = await Assert.ThrowsAsync<IdentityProfileException>(() =>
            service.GetProfileAsync(identity, CancellationToken.None));

        Assert.Equal(Now.AddDays(30), deletion.PurgeDueAt);
        Assert.Equal("authentication_required", exception.Code);
    }

    private static AuthenticatedIdentity TestIdentity(string subject) => new(
        ExternalIdentityProvider.Test,
        "https://test.majlis.local",
        subject,
        Now.AddMinutes(-1));

    private static IdentityProfileService CreateService(IUserAccountRepository repository) => new(
        repository,
        new RequiredConsentVersions("2026-08-26", "2026-08-26"),
        new FixedTimeProvider(Now));

    private static BootstrapProfileRequest BootstrapRequest(string displayName, string ageBand) => new(
        displayName,
        ageBand,
        "QA",
        "gulf",
        "qa",
        "ar",
        "2026-08-26",
        "2026-08-26");

    private sealed class FakeUserAccountRepository : IUserAccountRepository
    {
        public List<UserAccount> Users { get; } = [];

        public int SaveCount { get; private set; }

        public Task<UserAccount?> FindByIdentityAsync(
            ExternalIdentityProvider provider,
            string issuer,
            string subject,
            CancellationToken cancellationToken) => Task.FromResult(
                Users.SingleOrDefault(user => user.Identities.Any(identity =>
                    identity.Provider == provider &&
                    identity.Issuer == issuer &&
                    identity.Subject == subject)));

        public void Add(UserAccount user) => Users.Add(user);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
