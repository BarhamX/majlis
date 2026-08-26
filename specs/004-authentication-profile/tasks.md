# Tasks 004: Authentication, Authorization, and Profile

## Provider and Contracts

- [x] Fix Production V1 identity providers as Google Account and Sign in with Apple; exclude password, SMS, guest, and other social login.
- [ ] Add configuration validation and a test OIDC issuer/key set.
- [ ] Finalize identity/profile API contracts and RFC 7807 error codes.
- [ ] Implement explicit Google/Apple identity linking and collision rules without email auto-linking.

## Persistence and Domain

- [ ] Add reviewed migrations for users, profiles, preferences, role assignments, consents, and deletion requests.
- [ ] Implement age-band, display-name, privacy, and controlled-code validation.
- [ ] Implement deletion and retention state transitions.

## API and Authorization

- [ ] Validate tokens and idempotently resolve one local identity per provider/issuer/subject.
- [ ] Implement profile bootstrap, get, update, session-revocation coordination, and deletion endpoints.
- [ ] Implement least-privilege role policies and audited role assignment.
- [ ] Add auth/profile/deletion rate limits and safe errors.

## Android

- [ ] Implement provider-neutral login state, first-profile, returning-session, expiry, logout, recovery, privacy, and deletion flows against test identity.
- [ ] After `Game Ready`, configure and verify live Google Account and Sign in with Apple flows.
- [ ] Store tokens only in Android secure storage and clear them on logout/revocation.
- [ ] Verify Arabic/RTL, TalkBack, offline, cancellation, and error states.

## Verification

- [ ] Pass unit tests for `AUTH-003` through `AUTH-008` and `AUTH-013`.
- [ ] Pass integration tests for `AUTH-001`, `AUTH-002`, and `AUTH-009` through `AUTH-017`.
- [ ] Prove cross-user isolation and privileged-route authorization.
- [ ] Map all AUTH requirements in `docs/quality/requirements-to-tests.md` and update the handoff.
