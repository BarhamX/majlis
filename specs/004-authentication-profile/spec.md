# Spec 004: Authentication, Authorization, and Profile

## Goal

Provide a secure 13+ account lifecycle and private profile foundation for every stateful Majlis feature without making Majlis responsible for password storage.

## Scope

### In Scope

- Managed OpenID Connect authentication using Authorization Code with PKCE.
- First-login profile bootstrap, login, session renewal, logout, recovery, and revocation.
- Display name, age band, country, region/dialect, locale, privacy, leaderboard, and reminder preferences.
- User, moderator, content-editor, content-reviewer, publisher, and operations-admin authorization policies.
- Account deletion and retention enforcement.
- Cross-user isolation, operator MFA requirement, and auth abuse controls.

### Out of Scope

- Private Family Majlis membership.
- Social login providers not configured for V1.
- Collection of full date of birth, phone contacts, or precise location.
- Uploaded/custom profile avatars.

## Requirements

- **AUTH-001**: The Android app shall authenticate through a configured managed OIDC provider using Authorization Code with PKCE; Majlis shall not receive or store passwords.
- **AUTH-002**: The API shall accept only access tokens with a valid signature, configured issuer and audience, unexpired lifetime, and stable subject; it shall reject identity or role claims supplied in request bodies.
- **AUTH-003**: The first authenticated request shall idempotently create one local user for `(Issuer, Subject)` and require completion of the Majlis profile before gameplay mutations.
- **AUTH-004**: Registration shall require verified email at the identity provider, acceptance of current terms/privacy versions, and age-band attestation. A person declaring an age under 13 shall not receive a Majlis account.
- **AUTH-005**: A profile display name shall be 3-30 user-perceived characters after Unicode normalization, pass the configured safety validation, and be unique only where the UI explicitly requests a unique handle; V1 display names are otherwise non-unique.
- **AUTH-006**: Country shall use ISO 3166-1 alpha-2, locale shall use BCP 47, and region/dialect shall use controlled application codes. These preferences shall not select a different V1 Daily Majlis.
- **AUTH-007**: Profiles, attempts, progress, preferences, and activity shall be private. Public endpoints may expose only fields explicitly allowed by another feature spec.
- **AUTH-008**: Leaderboard visibility shall default to `private`. Only an `18_plus` user may opt into `global_weekly`.
- **AUTH-009**: Logout shall clear local credentials and revoke the provider session or refresh grant when supported. A user shall be able to revoke all Majlis sessions from profile settings.
- **AUTH-010**: Account recovery and email verification shall be provider-hosted and shall not reveal whether an email address has a Majlis account.
- **AUTH-011**: Privileged roles shall be assigned only through an audited operator process, never self-selected. Privileged accounts shall use provider MFA.
- **AUTH-012**: User data queries and mutations shall derive the acting user from the validated token and shall never accept a target user id for ordinary self-service operations.
- **AUTH-013**: An authenticated user and a public web flow shall support account-deletion requests. Revocation, public removal, purge, backup expiry, and audit retention shall meet `V1-DEC-009`.
- **AUTH-014**: Identity-provider authentication/recovery protections shall be verified in staging; Majlis profile/bootstrap and deletion endpoints shall enforce the per-IP/per-account limits in `docs/architecture/API_CONTRACTS.md` and safe RFC 7807 errors.
- **AUTH-015**: Changing a display name shall update future public rendering; historical moderation audit records shall retain only the immutable user id.

## Acceptance Criteria

- New, returning, expired-session, revoked-session, recovery, logout, and deletion journeys work on Android and in PostgreSQL-backed API tests.
- A token from a wrong issuer/audience, a forged role, and an expired token are rejected.
- Cross-user reads and writes are denied, including guessed UUIDs.
- A minor cannot opt into the leaderboard, and every new profile is private by default.
- Deletion is observable through an auditable state machine and verified against the documented deadlines.

## Dependencies

- `docs/product/v1-product-decisions.md`
- `docs/architecture/API_CONTRACTS.md`
- `docs/architecture/DATABASE_SCHEMA.md`
- `specs/009-production-operations/spec.md`
