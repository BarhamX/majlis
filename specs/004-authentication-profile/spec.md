# Spec 004: Authentication, Authorization, and Profile

## Goal

Provide a secure 13+ account lifecycle and private profile foundation for every stateful Majlis feature without making Majlis responsible for password storage.

## Scope

### In Scope

- Google Account through Android Credential Manager plus provider-supported Apple, Meta/Facebook Login, and Snapchat Login Kit flows.
- Explicit supported-provider identity linking without email-based automatic merging.
- An ephemeral signed test issuer with deterministic caller-selected subjects for local development and automated tests only.
- First-login profile bootstrap, login, session renewal, logout, recovery, and revocation.
- Display name, age band, country, region/dialect, locale, privacy, leaderboard, and reminder preferences.
- User, moderator, content-editor, content-reviewer, publisher, and operations-admin authorization policies.
- Account deletion and retention enforcement.
- Cross-user isolation, operator MFA requirement, and auth abuse controls.

### Out of Scope

- Private Family Majlis membership.
- Email/password, phone/SMS, guest, and providers other than Google, Apple, Meta, or Snapchat.
- Collection of full date of birth, phone contacts, or precise location.
- Uploaded/custom profile avatars.

## Requirements

- **AUTH-001**: Production V1 shall offer exactly four sign-in choices: Google Account through Android Credential Manager, Sign in with Apple, Meta/Facebook Login, and Snapchat Login Kit. Each flow shall use the provider-supported native/system-browser integration, validate required state/nonce, and use PKCE where supported; Majlis shall not use an embedded login webview or receive/store passwords.
- **AUTH-002**: The API shall accept only allowlisted Google, Apple, Meta, or Snapchat identities whose provider-specific token/code validation succeeds, including configured issuer/API origin and audience/client id, signature or introspection, expiry, stable subject, and state/nonce/code flow where applicable; it shall reject identity or role claims supplied in request bodies.
- **AUTH-003**: The first authenticated request shall idempotently create one local identity for `(Provider, Issuer, Subject)`, associate it with one local user, and require completion of the Majlis profile before gameplay mutations.
- **AUTH-004**: Registration shall require verified email at the identity provider, acceptance of current terms/privacy versions, and age-band attestation. A person declaring an age under 13 shall not receive a Majlis account.
- **AUTH-005**: A profile display name shall be 3-30 user-perceived characters after Unicode normalization, pass the configured safety validation, and be unique only where the UI explicitly requests a unique handle; V1 display names are otherwise non-unique.
- **AUTH-006**: Country shall use ISO 3166-1 alpha-2, locale shall use BCP 47, and region/dialect shall use controlled application codes. These preferences shall not select a different V1 Daily Majlis.
- **AUTH-007**: Profiles, attempts, progress, preferences, and activity shall be private. Public endpoints may expose only fields explicitly allowed by another feature spec.
- **AUTH-008**: Leaderboard visibility shall default to `private`. Only an `18_plus` user may opt into `global_weekly`.
- **AUTH-009**: Logout shall clear Majlis credentials on the device without signing the person out of their Google or Apple account globally. Revoking all Majlis sessions shall invalidate previously accepted Majlis sessions; a later explicit provider sign-in may create a new session.
- **AUTH-010**: Account recovery and verification shall be owned by Google, Apple, Meta, or Snapchat. Majlis support and public deletion responses shall not reveal whether an email address or provider identity has a Majlis account.
- **AUTH-011**: Privileged roles shall be assigned only through an audited operator process, never self-selected. Privileged accounts shall use provider MFA.
- **AUTH-012**: User data queries and mutations shall derive the acting user from the validated token and shall never accept a target user id for ordinary self-service operations.
- **AUTH-013**: An authenticated user and a public web flow shall support account-deletion requests. Majlis shall delete local identities, revoke provider authorization where required without deleting the person's external account, and meet the revocation, public-removal, purge, backup-expiry, and audit-retention rules in `V1-DEC-009`.
- **AUTH-014**: Identity-provider authentication/recovery protections shall be verified in staging; Majlis profile/bootstrap and deletion endpoints shall enforce the per-IP/per-account limits in `docs/architecture/API_CONTRACTS.md` and safe RFC 7807 errors.
- **AUTH-015**: Changing a display name shall update future public rendering; historical moderation audit records shall retain only the immutable user id.
- **AUTH-016**: An authenticated user may link another supported provider only after fresh proof from both sessions. A provider identity may belong to one local user, at most one identity per provider may be linked, at least one identity must remain, collisions shall fail safely, and identities shall never be merged by email equality.
- **AUTH-017**: An ephemeral signed test issuer with deterministic caller-selected subjects may be enabled only in Development and Testing. Production startup shall fail if test issuer, test signing keys, or bypass authentication is configured.

## Acceptance Criteria

- New, returning, linked-provider, expired-session, revoked-session, recovery, logout, and deletion journeys work for Google, Apple, Meta, and Snapchat before Production V1 release.
- A token from an unsupported/wrong issuer or audience, a forged role, an invalid nonce/code flow, and an expired token are rejected.
- Cross-user reads and writes are denied, including guessed UUIDs.
- A minor cannot opt into the leaderboard, and every new profile is private by default.
- Deletion is observable through an auditable state machine and verified against the documented deadlines.
- Before `Game Ready`, local and automated flows may use only the test issuer. Google, Apple, Meta, and Snapchat credentials and live provider tests are intentionally deferred to the post-`Game Ready` logistics phase.

## Dependencies

- `docs/product/v1-product-decisions.md`
- `docs/architecture/API_CONTRACTS.md`
- `docs/architecture/DATABASE_SCHEMA.md`
- `specs/009-production-operations/spec.md`
