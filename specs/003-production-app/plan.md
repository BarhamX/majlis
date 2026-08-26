# Plan 003: Majlis Production App

## Objective

Coordinate all feature slices into one complete Production V1. The order below minimizes rework while requiring every stage to remain runnable and tested.

## Delivery Sequence

1. Finish PostgreSQL persistence, migrations, health checks, and integration-test infrastructure.
2. Implement the provider-neutral local identity/profile/authorization foundation from Spec 004 with the Development/Testing issuer; defer live Google/Apple configuration.
3. Complete attempt submission, scoring, streaks, history, and share-result contracts.
4. Build the Flutter Android foundation and the complete daily journey.
5. Declare the internal `Game Ready` milestone only when the persisted Arabic/RTL daily journey and mapped core tests satisfy `V1-DEC-011`.
6. Implement Specs 005-007 locally: leaderboard, share-card/link routing with configurable placeholder hosts, and user-controlled reminders.
7. Implement Community Majlis with premoderation, reporting, blocking, and appeals from the start.
8. Implement Spec 008 authenticated content and moderation operations.
9. After `Game Ready`, configure live Google/Apple identity, hosting, public domains, verified App Links, production signing, staging, and Spec 009 operations/release gates; this may proceed alongside remaining local feature work without weakening either gate.
10. Validate the complete traceability matrix end to end and produce the Android release build and deployment runbook.

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
- Manual cultural review plus the measurable accessibility and Android device matrix in Spec 009.

## Release Gate

No single feature-spec completion constitutes Production V1. Release requires every mandatory item in `tasks.md`, all critical checks passing, and the handoff containing no unresolved release blocker.

`Game Ready` is an internal sequencing gate, not a reduced release or production claim.
