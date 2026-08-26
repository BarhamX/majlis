# Tasks 003: Majlis Production App

This is the release-wide checklist. Feature task files provide implementation detail; this file determines whether the full app is delivered.

## Specification Coverage

- [x] Add focused specs for authentication/profile and authorization.
- [x] Add focused specs for leaderboard, sharing/deep links, and notifications.
- [x] Add focused specs for content administration and production operations.
- [x] Reconcile `API_CONTRACTS.md` and `DATABASE_SCHEMA.md` with every approved Production V1 feature before implementation.
- [x] Add requirement IDs and the release-wide requirement-to-test traceability matrix.

## Persistent Platform

- [x] Configure PostgreSQL and EF Core in Infrastructure.
- [ ] Add explicit reviewed migrations for all production entities.
- [ ] Add a forward migration making source notes mandatory and introducing localized immutable content revisions.
- [x] Add idempotent development/test seed and content initialization.
- [x] Add database-backed health checks and configuration validation.
- [x] Replace production use of the in-memory Daily Majlis repository.
- [x] Add PostgreSQL-backed integration tests.

## Identity and Profile

- [ ] Implement registration, login, logout, session renewal, and account recovery.
- [ ] Implement authorization policies and least-privilege admin/moderator roles.
- [ ] Implement profile, region/dialect, privacy, and notification preferences.
- [ ] Test cross-user data isolation and privileged-route authorization.
- [ ] Complete all required tasks in `specs/004-authentication-profile/tasks.md`.

## Daily Game

- [ ] Complete all required tasks in `specs/001-playable-daily-majlis/tasks.md`.
- [ ] Persist exactly one scored attempt per user and challenge.
- [ ] Implement XP, current/longest streak, missed-day, and duplicate-award rules.
- [ ] Implement attempt history and spoiler-safe share result.
- [x] Resolve and test the content scheduling timezone.

## Android Application

- [ ] Create the Flutter app with feature-first structure, Riverpod, routing, theme, and localization resources.
- [ ] Implement onboarding, authentication, Today, challenge, result, cultural card, profile, progress, and leaderboard screens.
- [ ] Implement native sharing, inbound deep links, and user-controlled reminders.
- [ ] Implement loading, empty, offline, validation, retry, and completed states.
- [ ] Add Flutter unit, widget, golden, and end-to-end integration tests.
- [ ] Validate Arabic/RTL, accessibility, and every Android device-matrix row in Spec 009.
- [ ] Complete all required tasks in Specs 005-007.

## Community and Safety

- [ ] Complete all required tasks in `specs/002-community-majlis/tasks.md`.
- [ ] Add community rules, rate limiting, moderation queue, and auditable moderation actions.
- [ ] Verify hidden/removed content and private data cannot leak through public APIs.
- [ ] Implement blocking, appeals, minor-safety rules, and premoderation.

## Content Operations

- [ ] Implement authenticated content creation and editing.
- [ ] Require source notes, region/dialect tags, difficulty, and editorial status.
- [ ] Implement review, scheduling, publishing, correction, and unpublishing workflows.
- [ ] Provide the protected browser-based internal admin interface defined by Spec 008.
- [ ] Complete all required tasks in `specs/008-content-moderation-operations/tasks.md`.

## Production Operations

- [ ] Add structured logging, safe error handling, metrics, and product analytics.
- [ ] Add rate limiting and security headers/policies appropriate to each endpoint.
- [ ] Document local setup, production configuration, secrets, deployment, and rollback.
- [ ] Document and verify PostgreSQL backup and restore.
- [ ] Deploy and verify the production-shaped staging environment defined by `OPS-001`.
- [ ] Complete all measurable gates in `specs/009-production-operations/tasks.md`.

## Release Validation

- [ ] Pass backend unit and integration suites.
- [ ] Pass Flutter analysis, unit, widget, golden, and integration suites.
- [ ] Complete the end-to-end new-user, returning-user, publisher, and moderator journeys.
- [ ] Produce and install a reproducible signed Android release build.
- [ ] Complete cultural-content, privacy, security, moderation, accessibility, and performance review.
- [ ] Update `docs/ai-context/HANDOFF.md` with final release evidence and no unresolved release blocker.
