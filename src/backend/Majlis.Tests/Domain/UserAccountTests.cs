using Majlis.Domain.Identity;

namespace Majlis.Tests.Domain;

public sealed class UserAccountTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_AddsTheInitialExternalIdentity()
    {
        var account = UserAccount.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ExternalIdentityProvider.Meta,
            "https://www.facebook.com",
            "meta-subject",
            Now);

        var identity = Assert.Single(account.Identities);
        Assert.Equal(ExternalIdentityProvider.Meta, identity.Provider);
        Assert.Equal("https://www.facebook.com", identity.Issuer);
        Assert.Equal("meta-subject", identity.Subject);
        Assert.Equal(UserAccountStatus.Active, account.Status);
    }

    [Theory]
    [InlineData(ExternalIdentityProvider.Google)]
    [InlineData(ExternalIdentityProvider.Apple)]
    [InlineData(ExternalIdentityProvider.Meta)]
    [InlineData(ExternalIdentityProvider.Snapchat)]
    [InlineData(ExternalIdentityProvider.Test)]
    public void Create_SupportsEveryConfiguredProvider(ExternalIdentityProvider provider)
    {
        var account = UserAccount.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            provider,
            $"https://issuer.example/{provider}",
            "subject",
            Now);

        Assert.Equal(provider, Assert.Single(account.Identities).Provider);
    }

    [Fact]
    public void LinkIdentity_WhenProviderAlreadyLinked_RejectsDuplicate()
    {
        var account = UserAccount.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ExternalIdentityProvider.Google,
            "https://accounts.google.com",
            "google-subject",
            Now);

        var exception = Assert.Throws<InvalidOperationException>(() => account.LinkIdentity(
            Guid.NewGuid(),
            ExternalIdentityProvider.Google,
            "https://accounts.google.com",
            "another-subject",
            Now));

        Assert.Contains("already linked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompleteProfile_NormalizesValuesAndDefaultsToPrivate()
    {
        var account = CreateTestAccount();

        var profile = account.CompleteProfile(
            "  مريم  ",
            AgeBand.Adult18Plus,
            "qa",
            "gulf",
            "qa",
            "ar",
            Now);

        Assert.Equal("مريم", profile.DisplayName);
        Assert.Equal("QA", profile.CountryCode);
        Assert.Equal("gulf", profile.RegionCode);
        Assert.Equal("qa", profile.DialectCode);
        Assert.Equal("ar", profile.Locale);
        Assert.Equal(LeaderboardVisibility.Private, profile.LeaderboardVisibility);
        Assert.False(account.Preferences.ReminderEnabled);
    }

    [Fact]
    public void SetLeaderboardVisibility_WhenMinorOptsIn_RejectsChange()
    {
        var account = CreateTestAccount();
        var profile = account.CompleteProfile(
            "نورة",
            AgeBand.Minor13To17,
            "QA",
            "gulf",
            "qa",
            "ar",
            Now);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            profile.SetLeaderboardVisibility(LeaderboardVisibility.GlobalWeekly, Now));

        Assert.Contains("18", exception.Message, StringComparison.Ordinal);
        Assert.Equal(LeaderboardVisibility.Private, profile.LeaderboardVisibility);
    }

    [Fact]
    public void CompleteProfile_WhenCountryCodeIsUnknown_RejectsValue()
    {
        var account = CreateTestAccount();

        var exception = Assert.Throws<ArgumentException>(() => account.CompleteProfile(
            "Mariam",
            AgeBand.Adult18Plus,
            "ZZ",
            "gulf",
            "qa",
            "ar",
            Now));

        Assert.Contains("ISO 3166-1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RequestDeletion_IsIdempotentAndRevokesExistingAuthentication()
    {
        var account = CreateTestAccount();

        var first = account.RequestDeletion(Guid.NewGuid(), Now);
        var second = account.RequestDeletion(Guid.NewGuid(), Now.AddMinutes(1));

        Assert.Same(first, second);
        Assert.Equal(UserAccountStatus.DeletionPending, account.Status);
        Assert.Equal(Now, account.AuthenticationNotBefore);
        Assert.Equal(Now.AddDays(30), first.PurgeDueAt);
    }

    private static UserAccount CreateTestAccount() => UserAccount.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        ExternalIdentityProvider.Test,
        "https://test.majlis.local",
        "test-subject",
        Now);
}
