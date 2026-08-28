# Tasks 001: Playable Daily Majlis

## Backend Foundation

- [x] Create .NET solution under `src/backend` with projects: Api, Application, Domain, Infrastructure, Contracts, Tests.
- [x] Add project references according to clean architecture boundaries.
- [x] Add health endpoint to `Majlis.Api`.
- [x] Add PostgreSQL and EF Core packages to Infrastructure.
- [x] Add application configuration for database connection string.

## Domain Model

- [x] Create `DailyMajlis` entity.
- [x] Create `Challenge` entity.
- [x] Create `ChallengeOption` entity.
- [x] Create `UserAttempt` entity.
- [x] Create the `UserProgress` aggregate for lifetime XP, current streak, longest streak, and last completed publish date; do not create a separate `UserStreak` entity.
- [x] Create immutable XP ledger entity.
- [x] Add domain rule: exactly one correct option per multiple-choice challenge.
- [x] Add domain rule: one scored attempt per user per Daily Majlis, accepted only for the current UTC-date `published` challenge.

## Backend Use Cases

- [x] Implement query: get today's Daily Majlis.
- [x] Implement command: submit a challenge answer only for the current UTC-date `published` challenge.
- [x] Implement scoring service.
- [x] Implement streak updates through the single `UserProgress` aggregate.
- [x] Implement duplicate attempt handling.
- [x] Implement idempotency-key replay and concurrent submission handling.
- [x] Implement UTC published-content-day streak calculation.
- [x] Implement owned attempt result retrieval and newest-first opaque-cursor attempt history.
- [x] Add spoiler-safe share summary generation.

## Backend API

- [x] Add `GET /api/v1/daily-majlis/today`.
- [x] Add `POST /api/v1/challenges/{challengeId}/attempts`.
- [x] Add `GET /api/v1/attempts/{attemptId}` and `GET /api/v1/me/attempts`.
- [x] Add API response contracts.
- [x] Confirm today's endpoint does not expose correct answer.

## Backend Tests

- [x] Test scoring correct answer.
- [x] Test scoring incorrect answer.
- [x] Test duplicate attempt does not duplicate XP.
- [x] Test first completion updates streak.
- [x] Test missed day resets streak.
- [x] Test missing unpublished content day does not reset streak.
- [x] Test exact 10/15 XP awards and immutable ledger uniqueness.
- [x] Test idempotency-key mismatch and concurrent requests.
- [ ] Test historical, future, superseded-correction, scheduled, draft, and unpublished challenges cannot receive a new attempt.
- [x] Test stable `attempt_not_found` ownership non-enumeration, opaque-cursor stability, stored result locale/snapshots, and result-revision preservation after correction/unpublishing.
- [x] Test today's endpoint hides correct answer.

## Flutter Foundation

- [ ] Create Flutter app under `apps/mobile`.
- [ ] Add feature-first folder structure.
- [ ] Add Riverpod.
- [ ] Add GoRouter.
- [ ] Add theme tokens from `docs/design/THEME.md`.

## Flutter Daily Flow

- [ ] Create Today's Majlis screen.
- [ ] Create Challenge card component.
- [ ] Create answer option component.
- [ ] Create loading, error, unanswered, and completed states.
- [ ] Implement answer submission flow.
- [ ] Create Result screen.
- [ ] Create streak/XP display.
- [ ] Create share card component.

## Validation

- [x] Run backend tests.
- [ ] Run Flutter analyzer.
- [ ] Run Flutter tests.
- [ ] Manually complete daily loop on Android emulator.
- [x] Map `DLY-*`, `ATT-*`, and `PROG-*` requirements in `docs/quality/requirements-to-tests.md`.
- [x] Update `docs/ai-context/HANDOFF.md`.
