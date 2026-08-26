# Plan 003: Majlis Production App

## Objective

Coordinate all feature slices into one complete Production V1. The order below minimizes rework while requiring every stage to remain runnable and tested.

## Delivery Sequence

1. Finish PostgreSQL persistence, migrations, health checks, and integration-test infrastructure.
2. Implement authentication, authorization, profile, preferences, and account recovery.
3. Complete attempt submission, scoring, streaks, history, and share-result contracts.
4. Build the Flutter Android foundation and the complete daily journey.
5. Implement leaderboard, sharing/deep links, and user-controlled notifications.
6. Implement Community Majlis with reporting and moderation from the start.
7. Implement authenticated content and moderation operations.
8. Add analytics, observability, rate limiting, security hardening, and backup/restore.
9. Validate the complete system end to end and produce the Android release build and deployment runbook.

## Architecture

- Preserve API, Application, Domain, Infrastructure, Contracts, and Tests boundaries in .NET.
- Keep Flutter feature-first and use Riverpod for state.
- Use PostgreSQL for all production state; in-memory repositories are test/development aids only.
- Keep mobile contracts spoiler-safe and all privileged content/operations server-side.
- Add or amend a focused feature spec before implementing behavior not already specified.

## Validation Strategy

- Domain/application unit tests for business rules.
- PostgreSQL-backed API integration tests for persistence, uniqueness, authorization, and response safety.
- Flutter unit/widget tests for state and critical screens.
- Android integration tests for the complete user journey.
- Operational tests for migrations, health, logging, backup/restore, deployment, and rollback.
- Manual cultural, accessibility, and representative-device review before release.

## Release Gate

No single feature-spec completion constitutes Production V1. Release requires every mandatory item in `tasks.md`, all critical checks passing, and the handoff containing no unresolved release blocker.
