# Majlis Acceptance Criteria

These release-wide criteria summarize feature specifications. Exact rules and requirement IDs live in `specs/001-playable-daily-majlis/`, `specs/002-community-majlis/`, and `specs/004-authentication-profile/` through `specs/009-production-operations/`.

## Account and Profile

- WHEN a user signs in with a validated Google Account or Apple identity for the first time, THE SYSTEM SHALL create one local identity/user and then require a profile with display name and default preferences.
- WHEN an authenticated user links Google and Apple, THE SYSTEM SHALL require fresh proof from both and SHALL NOT merge accounts by email equality.
- WHEN a user selects region preference, THE SYSTEM SHALL save it to the profile.
- WHEN a person self-attests that they are under 13, THE SYSTEM SHALL prevent account creation.
- WHEN a user requests deletion, THE SYSTEM SHALL revoke access and meet the deletion and retention deadlines in `docs/product/v1-product-decisions.md`.
- WHEN a user is unauthenticated, THE SYSTEM SHALL show authentication or a safe deep-link landing state without returning challenge options.

## Daily Majlis

- WHEN the app opens, THE SYSTEM SHALL request the one published Daily Majlis for the current UTC content day.
- WHEN no Daily Majlis is published for today, THE SYSTEM SHALL return a safe fallback response instead of crashing.
- WHEN pre-attempt Daily Majlis content is returned, THE SYSTEM SHALL include the localized title, topic, question, options, and discussion prompt but no correct option, explanation, answer-derived result, or internal source notes.
- WHEN a regional preference is saved, THE SYSTEM SHALL continue serving the same official V1 edition.

## Challenge Submission

- WHEN a user submits an answer for today's challenge, THE SYSTEM SHALL validate the selected option.
- WHEN the first answer is accepted, THE SYSTEM SHALL return result, correct answer, explanation, 10 completion XP plus 5 XP if correct, and streak status.
- WHEN the first answer is incorrect, THE SYSTEM SHALL still complete the content day; V1 SHALL NOT offer an answer retry.
- WHEN duplicate or concurrent submissions occur, THE SYSTEM SHALL preserve one attempt and one XP/streak mutation and return or identify the original outcome.

## Streaks

- WHEN a user completes a Daily Majlis for the first time, THE SYSTEM SHALL update current streak using its UTC `PublishDate`.
- WHEN a user skips an eligible published content day, THE SYSTEM SHALL reset current streak on the next completion.
- WHEN the service failed to publish an eligible content day, THE SYSTEM SHALL NOT break a user's streak for that missing day.
- WHEN current streak exceeds longest streak, THE SYSTEM SHALL update longest streak.

## Discussion

- WHEN a user submits a response, THE SYSTEM SHALL store it as `pending` and show it only to its author and moderators until approval.
- WHEN a moderator approves a response, THE SYSTEM SHALL make it visible in normal public results.
- WHEN a response is reported, THE SYSTEM SHALL mark it for moderation review.
- WHEN a response is hidden by moderation, THE SYSTEM SHALL not show it in public discussion results.
- WHEN either user has blocked the other, THE SYSTEM SHALL prevent their comments and reactions from being exposed to each other.
- WHEN a user appeals an eligible moderation action, THE SYSTEM SHALL record and resolve the appeal without altering the original audit event.

## Sharing

- WHEN a user completes today's challenge, THE SYSTEM SHALL allow generating a result card.
- WHEN a result is shared, THE SYSTEM SHALL never include the correct option, explanation, user identity, or private progress in the card or link.
- WHEN a recipient opens a supported link, THE SYSTEM SHALL route them to the referenced current Daily Majlis or a safe expired/invalid fallback.

## Progress and Friendly Competition

- WHEN a user completes a scored challenge, THE SYSTEM SHALL persist the resulting XP and leaderboard contribution exactly once.
- WHEN leaderboard data is shown, THE SYSTEM SHALL include only opted-in adult users and expose approved display name, rank, and weekly XP.
- WHEN rankings are presented, THE APP SHALL use friendly, non-shaming language.

## Android Experience

- WHEN a user follows any core journey, THE APP SHALL provide usable loading, empty, validation, offline, retry, and completed states where applicable.
- WHEN the app is opened through a supported Majlis link, THE APP SHALL route to the intended safe destination after authentication if required.
- WHEN reminders are enabled, THE APP SHALL schedule at most one local daily notification at the user's chosen local time.
- WHEN reminders are disabled, THE APP SHALL cancel scheduled reminders immediately.

## Admin Content

- WHEN an editor submits content for review, THE SYSTEM SHALL require a complete Arabic translation, challenge, correct answer, explanation, discussion prompt, provenance tags, and non-empty internal source notes.
- WHEN a publisher schedules content, THE SYSTEM SHALL prevent more than one scheduled or published official Daily Majlis for the same UTC date.
- WHEN content is published, THE SYSTEM SHALL require an approved revision and auditable reviewer/publisher actions.
- WHEN content is unpublished, THE SYSTEM SHALL stop serving it to users.

## Production Operations

- WHEN the service starts in a clean environment, THE SYSTEM SHALL apply or clearly expose the required migration procedure and validate required configuration.
- WHEN the API or database is unhealthy, THE SYSTEM SHALL expose a useful protected operational signal without leaking secrets.
- WHEN the API restarts, THE SYSTEM SHALL retain all production accounts, content, attempts, progress, comments, reports, and moderation state.
- WHEN a release is produced, THE TEAM SHALL be able to reproduce the backend deployment and installable Android build from documented steps.
- WHEN release readiness is evaluated, THE SYSTEM and app SHALL meet every measurable threshold in `specs/009-production-operations/spec.md` and every mapped check in `docs/quality/requirements-to-tests.md`.
