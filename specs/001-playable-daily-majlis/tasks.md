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
- [ ] Create `UserAttempt` entity.
- [ ] Create `UserStreak` entity.
- [ ] Create immutable XP ledger entity.
- [x] Add domain rule: exactly one correct option per multiple-choice challenge.
- [ ] Add domain rule: one scored attempt per user per challenge.

## Backend Use Cases

- [x] Implement query: get today's Daily Majlis.
- [ ] Implement command: submit challenge answer.
- [ ] Implement scoring service.
- [ ] Implement streak service.
- [ ] Implement duplicate attempt handling.
- [ ] Implement idempotency-key replay and concurrent submission handling.
- [ ] Implement UTC published-content-day streak calculation.
- [ ] Implement own attempt history.
- [ ] Add spoiler-safe share summary generation.

## Backend API

- [x] Add `GET /api/v1/daily-majlis/today`.
- [ ] Add `POST /api/v1/challenges/{challengeId}/attempts`.
- [ ] Add `GET /api/v1/attempts/{attemptId}` and `GET /api/v1/me/attempts`.
- [x] Add API response contracts.
- [x] Confirm today's endpoint does not expose correct answer.

## Backend Tests

- [ ] Test scoring correct answer.
- [ ] Test scoring incorrect answer.
- [ ] Test duplicate attempt does not duplicate XP.
- [ ] Test first completion updates streak.
- [ ] Test missed day resets streak.
- [ ] Test missing unpublished content day does not reset streak.
- [ ] Test exact 10/15 XP awards and immutable ledger uniqueness.
- [ ] Test idempotency-key mismatch and concurrent requests.
- [ ] Test attempt ownership and result-revision preservation.
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
- [ ] Map `DLY-*`, `ATT-*`, and `PROG-*` requirements in `docs/quality/requirements-to-tests.md`.
- [x] Update `docs/ai-context/HANDOFF.md`.
