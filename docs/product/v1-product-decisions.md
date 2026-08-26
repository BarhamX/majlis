# Majlis Production V1 Decisions

## Purpose

This document closes cross-feature decisions that otherwise allow incompatible implementations. Feature specifications own detailed behavior; this file owns the release-wide interpretation.

## Normative Decisions

### V1-DEC-001 - One Global Content Day

- `PublishDate` is the canonical UTC calendar date.
- Production V1 publishes one official Daily Majlis for each UTC date.
- A user's country, region, dialect, device timezone, or reminder time does not select a different Daily Majlis in V1.
- Streak eligibility is based on consecutive published `PublishDate` values, not device-local midnight.
- If Majlis fails to publish an eligible content day, that missing day does not break a user's streak.

### V1-DEC-002 - Regional Focus Is Editorial, Not Segmentation

- The initial editorial and beta focus is Qatar/Gulf, consistent with the business launch sequence.
- Region and dialect tags describe cultural provenance and help editors balance the calendar.
- Profile region and dialect preferences are saved but do not filter or replace the official Daily Majlis in V1.
- Region-specific editions and regional leaderboards are post-V1 capabilities and require a new specification plus a replacement for the global publish-date uniqueness rule.

### V1-DEC-003 - Arabic Is the Required Launch Locale

- Arabic (`ar`) is the required consumer UI and content locale for Production V1, and the app starts in Arabic unless the user has already selected another supported locale.
- UI layout must support RTL from the first Flutter screen. "Arabic-ready" does not mean that Arabic may be deferred.
- English (`en`) UI resources may ship, but English content translations are optional in V1.
- Published cultural content must have a complete Arabic translation. APIs use BCP 47 language tags and may fall back from a regional Arabic tag such as `ar-QA` to `ar`.
- Content fields are localized in translation records rather than duplicated language-specific columns.
- The launch Arabic font is Noto Sans Arabic, bundled with the app under its license. A later brand change must pass the same readability and accessibility checks.

### V1-DEC-004 - Family Connection Without Private Groups

- Private Family Majlis groups, group membership, private group discussions, and family-only leaderboards are post-V1.
- V1 supports family and friend connection through user-initiated external sharing, safe deep links, and an opt-in global weekly leaderboard.
- No V1 copy or journey may imply that private groups exist.

### V1-DEC-005 - External Identity Providers

- Production V1 supports Google Account, Sign in with Apple, Meta/Facebook Login, and Snapchat Login Kit. Email/password, phone/SMS, guest, and other providers are out of scope.
- Android uses Google Credential Manager and provider-supported native/system-browser flows for Apple, Meta, and Snapchat with required state/nonce validation and PKCE where supported; embedded login webviews are prohibited.
- Majlis does not store user passwords or implement password reset tokens. Any provider credential retained solely to satisfy provider-required revocation is encrypted with a managed key, never logged, and deleted after revocation/account purge.
- Google, Apple, Meta, and Snapchat own account verification and recovery. Majlis stores the provider, issuer, and stable subject but does not use email as an account key.
- The backend performs the selected provider's required verification: signed-token issuer/audience/signature validation or server-side code exchange/introspection, plus expiry, state/nonce/PKCE checks where applicable, before mapping the stable provider subject to a local `User`.
- One Majlis user may explicitly link one identity from each supported provider after authenticating both the current and added identities. Majlis never auto-links accounts by matching email, including Apple private-relay addresses.
- Provider adapters and configuration must not change Majlis domain identifiers or public feature contracts.
- V1 challenge options, attempts, progress, leaderboard, and discussion require an authenticated completed profile. Public deep-link landing pages may explain Majlis but do not expose the challenge options.

### V1-DEC-006 - One Final Attempt and Non-Shaming Rewards

- Each user gets one final scored submission per challenge and no answer retry in V1.
- The first accepted attempt earns 10 completion XP; a correct answer earns 5 additional XP.
- Both correct and incorrect first attempts complete the content day for streak purposes.
- Replays, duplicate requests, and concurrent submissions return or locate the original result and never award XP or streak progress again.
- The correct option and explanation are revealed only after the first attempt is accepted.

### V1-DEC-007 - Premoderated Public Discussion

- New comments start as `pending` and are visible only to the author and moderators.
- A moderator must approve a comment before it becomes `visible` in public discussion queries.
- Reports never make hidden content public and every moderation transition is audited.
- Users can block another user without notifying them. Public queries and reactions exclude either side of a block relationship.
- Private messaging is not in V1.

### V1-DEC-008 - Age and Privacy Baseline

- Majlis is for users aged 13 or older. Registration requires age-band attestation (`13_17` or `18_plus`); full date of birth is not collected.
- Users under 13 cannot create an account. Jurisdictions that require additional consent for minors must not be enabled until the required consent flow is specified and implemented.
- Minor accounts cannot appear on the public leaderboard in V1.
- Profiles, attempt history, activity, region/dialect preferences, and notification preferences are private by default.
- Public surfaces may show only an approved display name and the fields explicitly allowed by the owning feature specification.
- V1 uses generated initials only; uploaded/custom avatars are post-V1.

### V1-DEC-009 - Account Deletion and Retention

- An authenticated user can request account deletion in the Android app; an equivalent public web path is provided for users who cannot sign in.
- The request immediately revokes Majlis sessions, hides authored comments, and removes the user from public leaderboards.
- Active profile, attempts, progress, preferences, and comment content are deleted or irreversibly anonymized within 30 days.
- Deleted data expires from ordinary backups within 35 additional days and cannot be restored into the live service without reapplying deletion tombstones.
- Security and moderation audit records may retain a pseudonymous user reference for up to 180 days. They must not retain email, display name, or comment text unless a documented legal hold applies.
- Operational logs are retained for 30 days and product analytics for 13 months, using pseudonymous identifiers and no cultural-answer text or comment text.

### V1-DEC-010 - Reminder and Leaderboard Boundaries

- V1 daily reminders are local Android notifications scheduled at the user's chosen local time and timezone. They are off by default and can be disabled in one action.
- Majlis sends at most one daily challenge reminder. It sends no streak-loss warning, re-engagement nag, or notification based on another user's activity.
- The V1 leaderboard is one global UTC weekly board. Adult users must opt in; it shows approved display name, rank, and weekly XP only.
- The board returns the top 100 entries and the requesting user's own entry when eligible. Equal XP receives the same displayed rank.

### V1-DEC-011 - Game-Ready Before Deployment Logistics

- The internal `Game Ready` milestone means the Arabic/RTL Flutter daily journey runs end to end against the local .NET/PostgreSQL backend with deterministic test identities from the ephemeral Development/Testing issuer: profile, today's challenge, one final attempt, result/cultural card, XP, streak, and automated core-flow tests.
- Development/test identity must be impossible to enable in Production configuration.
- Google, Apple, Meta, and Snapchat production credentials/callback configuration, hosting procurement, public domains, verified App Links, production signing, production-shaped staging, monitoring, backup, and deployment work are deliberately deferred until after `Game Ready`.
- Provider-facing interfaces, configurable URL boundaries, and release requirements are specified now so game code does not assume a vendor credential, host, or domain.
- This sequencing decision does not remove the four production identity integrations, verified links, hosting, staging, or Spec 009 evidence from the Production V1 release gate.

## Change Control

Changing any decision above requires an explicit product decision, updates to every affected feature specification, API/schema reconciliation, traceability updates, and a handoff entry. An implementation task may not silently override this document.
