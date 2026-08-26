# Plan 004: Authentication, Authorization, and Profile

## Delivery Order

1. Select and prove a managed OIDC provider in staging against the requirements in `spec.md`.
2. Add local user/profile/preference, role, consent, and deletion persistence through explicit migrations.
3. Add token validation and local-user resolution behind Infrastructure interfaces.
4. Implement profile bootstrap, read/update, logout/revocation coordination, and deletion use cases.
5. Add role policies and audit privileged-role assignment.
6. Build Android authentication, profile completion, session recovery, privacy, and deletion states.
7. Verify abuse limits, cross-user isolation, revocation, and retention jobs.

## Architecture

- Identity-provider types stay in Infrastructure; Domain uses the local user id.
- Controllers derive the local user from authenticated context.
- Provider-hosted screens handle password entry, verification, and recovery.
- Profile and deletion state remain authoritative in PostgreSQL even if the external identity is unavailable.

## Validation

- Unit tests for profile rules and deletion transitions.
- PostgreSQL-backed integration tests using signed test tokens from a controlled test issuer.
- Contract tests for safe errors and claim handling.
- Android integration tests for first login, returning login, expiry, logout, and deletion.
