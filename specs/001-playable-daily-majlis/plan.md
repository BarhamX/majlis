# Plan 001: Playable Daily Majlis

This plan is the first daily-loop implementation slice within the complete Production V1. It is not a standalone release plan.

## Architecture

Backend owns challenge truth, scoring, streaks, and result generation. Flutter displays today's Majlis, collects answer selection, submits to backend, and renders the result/share UI.

## Backend Work

1. Create solution skeleton.
2. Add core domain entities.
3. Add EF Core DbContext.
4. Seed sample Daily Majlis.
5. Implement today's Daily Majlis query.
6. Implement answer submission command.
7. Implement transactional attempt, XP-ledger, and streak services with database uniqueness.
8. Add localized content records and spoiler-safe pre/post-attempt contracts.
9. Expose API endpoints and safe problem codes.
10. Add concurrency/idempotency and authorization tests.

## Flutter Work

1. Create Flutter app skeleton.
2. Add theme tokens.
3. Add routing.
4. Add Daily Majlis feature.
5. Add Challenge feature.
6. Add Result feature.
7. Add Share Card component.
8. Add API client.
9. Add basic tests.

## Data Model

Use the schema in `docs/architecture/DATABASE_SCHEMA.md`.

## API Contracts

Use the contracts in `docs/architecture/API_CONTRACTS.md`.

## Testing

- Unit test scoring.
- Unit test exact XP and UTC published-day streak rules.
- PostgreSQL integration test duplicate and concurrent attempt behavior.
- API test today's Majlis response does not expose correct answer.
- API test explanation, internal sources, review state, and answer statistics are absent before submission.
- API test cross-user attempt isolation and localized Arabic fallback.
- Flutter widget test for challenge state transitions.
