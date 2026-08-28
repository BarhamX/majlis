# Spec 001: Playable Daily Majlis

## Goal

Build the daily-game vertical slice of the complete Majlis app: a user can open today's Majlis, answer the daily cultural challenge, receive feedback, read a short explanation, update streak/XP, and generate a shareable result card.

This feature is an implementation milestone. It does not define or reduce the Production V1 delivery scope in `docs/product/full-app-scope.md`.

## Primary User Story

As an Arab user, I want to answer a short daily cultural challenge so that I can test my cultural knowledge, learn something quickly, and challenge my friends or family.

## Scope

### In Scope

- Fetch today's Daily Majlis.
- Display challenge and answer options.
- Submit one final answer with idempotent/concurrent handling.
- Server-side validation.
- Result and explanation.
- Exact XP and streak update.
- Attempt history and spoiler-safe share metadata.

### Out of Scope

- Comments and discussion implementation.
- Friend groups.
- Premium access.
- Advanced audio.
- Full admin UI.
- Authentication/profile implementation, which is specified by `specs/004-authentication-profile/`.

Items listed here are outside this feature slice only. Production V1 requirements remain governed by `specs/003-production-app/`.

## Requirements

### Daily Content

- **DLY-001**: The system shall select the one `published` Daily Majlis whose `PublishDate` equals the current UTC calendar date. Region, dialect, country, locale, and device timezone shall not select a different V1 edition.
- **DLY-002**: The response shall select a complete localized content set using `Accept-Language`, fall back from regional Arabic to `ar`, and return `Content-Language`. Arabic shall exist for every published item.
- **DLY-003**: Before an accepted attempt, the response may include localized title, topic, question, 2-4 options, difficulty, provenance tag, and discussion prompt. It shall not include correctness flags, correct option, explanation, internal source notes, review state, or answer-derived statistics.
- **DLY-004**: If no eligible publication exists, the API shall return `404` with problem code `daily_majlis_unavailable`; the app shall show a retryable, non-blaming fallback.
- **DLY-005**: If the user already attempted the challenge, today's response shall include only `hasAttempted` and `attemptId`; the app shall retrieve the authoritative result instead of allowing another answer.

### Attempt and Result

- **ATT-001**: Only an authenticated user with a completed Majlis profile may submit an option belonging to the requested challenge.
- **ATT-002**: `POST /challenges/{challengeId}/attempts` shall require an `Idempotency-Key` UUID. The first accepted request shall atomically create one `UserAttempt`, one XP-ledger entry, and the streak mutation.
- **ATT-003**: Replaying the same key and payload shall return the original result without mutation. Reusing a key with a different payload shall return `409 idempotency_key_reused`.
- **ATT-004**: A different key after an attempt already exists shall return `409 attempt_already_completed` with the existing `attemptId`. Concurrent requests shall converge on the same persisted attempt and award progress once.
- **ATT-005**: The first accepted response shall return the attempt id, correctness, correct option, localized explanation and cultural card, XP breakdown, and current/longest streak. Only this post-attempt contract may reveal the answer and learning content.
- **ATT-006**: The first accepted option is final. V1 shall not allow answer retries, replacement attempts, or rescoring.
- **ATT-007**: `GET /attempts/{attemptId}` and paginated `GET /me/attempts` shall return only the authenticated user's attempts and shall preserve the content revision used for each result.
- **ATT-008**: Attempt submission shall enforce the per-account and per-IP limits in `docs/architecture/API_CONTRACTS.md`, returning `429` and `Retry-After` without creating or modifying an attempt.

### XP and Streak

- **PROG-001**: The first accepted attempt shall award 10 completion XP plus 5 additional XP when correct. An incorrect attempt therefore awards 10 XP and a correct attempt 15 XP.
- **PROG-002**: The immutable XP ledger shall reference the attempt with a unique constraint so a challenge contributes once to lifetime and weekly totals.
- **PROG-003**: Both correct and incorrect accepted attempts complete the Daily Majlis `PublishDate` for streak purposes.
- **PROG-004**: Completing the next eligible published content day increments current streak; repeating the same day leaves it unchanged; skipping an eligible published day resets the next completion to 1.
- **PROG-005**: A UTC date for which no Daily Majlis was published is not an eligible content day and shall not break a streak.
- **PROG-006**: Longest streak shall be the maximum completed current streak and shall never decrease.

### Sharing

- **DLY-006**: An accepted result shall expose the non-sensitive metadata needed by `specs/006-sharing-deep-links/`; rendering and deep-link behavior are owned by that specification.

## Acceptance Criteria

- User can complete the full daily loop in 1-3 minutes.
- Backend tests cover exact XP, streak boundaries, missed publication days, idempotency, concurrency, and duplicate-attempt behavior.
- Flutter can show loading, success, error, and completed states.
- Correct answer and explanation are never sent before an accepted submission.
- Share card never spoils the answer in V1.
- Incorrect answers are final, award 10 XP, and advance the content-day streak.
- The attempt, XP ledger, and streak update survive API restart and are committed exactly once.
