# Tasks 004: Authentication, Authorization, and Profile

## Provider and Contracts

- [x] Fix Production V1 identity providers as Google Account, Sign in with Apple, Meta/Facebook Login, and Snapchat Login Kit; exclude password, SMS, guest, and other login.
- [x] Add environment validation and an ephemeral signed Development/Testing issuer.
- [x] Finalize provider-neutral local identity/profile API contracts and RFC 7807 error codes.
- [x] Implement supported-provider identity-linking domain invariants and persistence uniqueness without email auto-linking.
- [ ] After `Game Ready`, finalize provider-specific authorization-result contracts and live linking endpoints.

## Persistence and Domain

- [x] Add a reviewed migration for users, identities, profiles, preferences, role assignments, consents, and deletion requests.
- [x] Implement age-band, display-name, private-default, locale, country, and profile-code validation.
- [x] Implement deletion request and immediate authentication revocation.
- [ ] Implement purge, provider-revocation, backup-expiry, and legal-hold jobs/transitions.

## API and Authorization

- [x] Validate signed test tokens and idempotently resolve one local identity per provider/issuer/subject.
- [x] Implement profile bootstrap, get, update, session-revocation coordination, and deletion endpoints.
- [ ] Implement least-privilege role policies and audited role assignment.
- [ ] Add auth/profile/deletion rate limits and safe errors.

## Android

- [ ] Implement provider-neutral login state, first-profile, returning-session, expiry, logout, recovery, privacy, and deletion flows against test identity.
- [ ] After `Game Ready`, configure and verify live Google, Apple, Meta, and Snapchat flows.
- [ ] Store tokens only in Android secure storage and clear them on logout/revocation.
- [ ] Verify Arabic/RTL, TalkBack, offline, cancellation, and error states.

## Verification

- [x] Pass focused unit/functional tests for the implemented portions of `AUTH-003` through `AUTH-008`, `AUTH-013`, and `AUTH-017`.
- [ ] Pass integration tests for `AUTH-001`, `AUTH-002`, and `AUTH-009` through `AUTH-017`.
- [x] Prove self-scoped profile isolation through functional API tests.
- [ ] Prove privileged-route authorization after role-management endpoints exist.
- [x] Map all AUTH requirements in `docs/quality/requirements-to-tests.md` and update the handoff.
