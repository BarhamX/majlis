using Majlis.Domain.Identity;
using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using DeletionRequestEntity = Majlis.Domain.Identity.AccountDeletionRequest;

namespace Majlis.Tests.Infrastructure;

public sealed class IdentityPersistenceModelTests
{
    [Fact]
    public void Model_DefinesIdentityProfileAndDeletionTablesWithRequiredUniqueness()
    {
        var options = new DbContextOptionsBuilder<MajlisDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=model_only;Password=model_only")
            .Options;
        using var dbContext = new MajlisDbContext(options, TimeProvider.System);

        var model = dbContext.GetService<IDesignTimeModel>().Model;
        var identity = model.FindEntityType(typeof(UserIdentity));
        var account = model.FindEntityType(typeof(UserAccount));
        var profile = model.FindEntityType(typeof(UserProfile));
        var preferences = model.FindEntityType(typeof(UserPreferences));
        var deletion = model.FindEntityType(typeof(DeletionRequestEntity));

        Assert.NotNull(account);
        Assert.NotNull(profile);
        Assert.NotNull(preferences);
        Assert.NotNull(deletion);
        Assert.NotNull(identity);

        Assert.Contains(identity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(UserIdentity.Issuer), nameof(UserIdentity.Subject)]));
        Assert.Contains(identity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(UserIdentity.UserId), nameof(UserIdentity.Provider)]));

        var providerConstraint = Assert.Single(
            identity.GetCheckConstraints(),
            constraint => constraint.Name == "CK_UserIdentities_Provider");
        Assert.Contains("'google'", providerConstraint.Sql, StringComparison.Ordinal);
        Assert.Contains("'apple'", providerConstraint.Sql, StringComparison.Ordinal);
        Assert.Contains("'meta'", providerConstraint.Sql, StringComparison.Ordinal);
        Assert.Contains("'snapchat'", providerConstraint.Sql, StringComparison.Ordinal);
        Assert.Contains("'test'", providerConstraint.Sql, StringComparison.Ordinal);
    }
}
