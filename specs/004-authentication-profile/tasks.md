# Tasks 004: Authentication, Authorization, and Profile

## Provider and Contracts

- [ ] Select a managed OIDC provider and record issuer, audience, PKCE, recovery, revocation, MFA, SLA, and data-residency evidence.
- [ ] Add configuration validation and a test OIDC issuer/key set.
- [ ] Finalize identity/profile API contracts and RFC 7807 error codes.

## Persistence and Domain

- [ ] Add reviewed migrations for users, profiles, preferences, role assignments, consents, and deletion requests.
- [ ] Implement age-band, display-name, privacy, and controlled-code validation.
- [ ] Implement deletion and retention state transitions.

## API and Authorization

- [ ] Validate tokens and idempotently resolve one local user per issuer/subject.
- [ ] Implement profile bootstrap, get, update, session-revocation coordination, and deletion endpoints.
- [ ] Implement least-privilege role policies and audited role assignment.
- [ ] Add auth/profile/deletion rate limits and safe errors.

## Android

- [ ] Implement provider login, first-profile, returning-session, expiry, logout, recovery, privacy, and deletion flows.
- [ ] Store tokens only in Android secure storage and clear them on logout/revocation.
- [ ] Verify Arabic/RTL, TalkBack, offline, cancellation, and error states.

## Verification

- [ ] Pass unit tests for `AUTH-003` through `AUTH-008` and `AUTH-013`.
- [ ] Pass integration tests for `AUTH-001`, `AUTH-002`, and `AUTH-009` through `AUTH-015`.
- [ ] Prove cross-user isolation and privileged-route authorization.
- [ ] Map all AUTH requirements in `docs/quality/requirements-to-tests.md` and update the handoff.
