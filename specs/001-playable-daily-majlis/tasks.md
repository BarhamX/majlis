# Tasks 001: Playable Daily Majlis

## Backend Foundation

- [ ] Create .NET solution under `src/backend` with projects: Api, Application, Domain, Infrastructure, Contracts, Tests.
- [ ] Add project references according to clean architecture boundaries.
- [ ] Add health endpoint to `Majlis.Api`.
- [ ] Add PostgreSQL and EF Core packages to Infrastructure.
- [ ] Add application configuration for database connection string.

## Domain Model

- [ ] Create `DailyMajlis` entity.
- [ ] Create `Challenge` entity.
- [ ] Create `ChallengeOption` entity.
- [ ] Create `UserAttempt` entity.
- [ ] Create `UserStreak` entity.
- [ ] Add domain rule: exactly one correct option per multiple-choice challenge.
- [ ] Add domain rule: one scored attempt per user per challenge.

## Backend Use Cases

- [ ] Implement query: get today's Daily Majlis.
- [ ] Implement command: submit challenge answer.
- [ ] Implement scoring service.
- [ ] Implement streak service.
- [ ] Implement duplicate attempt handling.
- [ ] Add spoiler-safe share summary generation.

## Backend API

- [ ] Add `GET /api/v1/daily-majlis/today`.
- [ ] Add `POST /api/v1/challenges/{challengeId}/attempts`.
- [ ] Add API response contracts.
- [ ] Confirm today's endpoint does not expose correct answer.

## Backend Tests

- [ ] Test scoring correct answer.
- [ ] Test scoring incorrect answer.
- [ ] Test duplicate attempt does not duplicate XP.
- [ ] Test first completion updates streak.
- [ ] Test missed day resets streak.
- [ ] Test today's endpoint hides correct answer.

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

- [ ] Run backend tests.
- [ ] Run Flutter analyzer.
- [ ] Run Flutter tests.
- [ ] Manually complete daily loop on Android emulator.
- [ ] Update `docs/ai-context/HANDOFF.md`.
