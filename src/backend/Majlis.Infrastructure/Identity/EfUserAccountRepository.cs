using Majlis.Application.Identity;
using Majlis.Domain.Identity;
using Majlis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Majlis.Infrastructure.Identity;

internal sealed class EfUserAccountRepository(MajlisDbContext dbContext) : IUserAccountRepository
{
    public Task<UserAccount?> FindByIdentityAsync(
        ExternalIdentityProvider provider,
        string issuer,
        string subject,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsSplitQuery()
            .Include(user => user.Identities)
            .Include(user => user.Profile)
            .Include(user => user.Preferences)
            .Include(user => user.Consents)
            .Include(user => user.DeletionRequests)
            .SingleOrDefaultAsync(
                user => user.Identities.Any(identity =>
                    identity.Provider == provider &&
                    identity.Issuer == issuer &&
                    identity.Subject == subject),
                cancellationToken);
    }

    public void Add(UserAccount user) => dbContext.Users.Add(user);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_UserIdentities_Issuer_Subject",
            })
        {
            foreach (var entry in dbContext.ChangeTracker.Entries()
                         .Where(entry => entry.State == EntityState.Added)
                         .ToArray())
            {
                entry.State = EntityState.Detached;
            }

            throw new IdentityConflictException(exception);
        }
    }
}
