using Majlis.Domain.Identity;

namespace Majlis.Application.Identity;

public interface IUserAccountRepository
{
    Task<UserAccount?> FindByIdentityAsync(
        ExternalIdentityProvider provider,
        string issuer,
        string subject,
        CancellationToken cancellationToken);

    void Add(UserAccount user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
