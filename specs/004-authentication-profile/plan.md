# Plan 004: Authentication, Authorization, and Profile

## Delivery Order

1. Add local user/identity/profile/preference, role, consent, and deletion persistence through explicit migrations.
2. Add provider-neutral token validation/local-user resolution plus a deterministic Development/Testing issuer that fails closed in Production.
3. Implement profile bootstrap, read/update, identity-linking, logout/revocation coordination, and deletion use cases.
4. Add role policies and audit privileged-role assignment.
5. Build provider-neutral Android authentication state, profile completion, session recovery, privacy, and deletion states against test identity.
6. Complete the local `Game Ready` milestone and its automated tests.
7. In the post-`Game Ready` logistics phase, configure Google and Sign in with Apple adapters/credentials and verify both end to end in production-shaped staging.
8. Verify abuse limits, cross-user isolation, revocation, linking collisions, and retention jobs.

## Architecture

- Identity-provider types stay in Infrastructure; Domain uses the local user id.
- Controllers derive the local user from authenticated context.
- Google/Apple native or system-browser surfaces handle account authentication, verification, and recovery; Majlis never embeds their login pages.
- Provider adapters implement one Application boundary. Development/Testing uses a signed test adapter; Production permits only Google and Apple.
- Profile and deletion state remain authoritative in PostgreSQL even if the external identity is unavailable.

## Validation

- Unit tests for profile rules and deletion transitions.
- PostgreSQL-backed integration tests using signed test tokens from a controlled test issuer.
- Contract tests for safe errors and claim handling.
- Android integration tests for first login, returning login, linking, expiry, logout, and deletion; live Google/Apple tests run only after the logistics phase begins.
