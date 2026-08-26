using Majlis.Domain.Identity;

namespace Majlis.Application.Identity;

public sealed record AuthenticatedIdentity(
    ExternalIdentityProvider Provider,
    string Issuer,
    string Subject,
    DateTimeOffset IssuedAt);
