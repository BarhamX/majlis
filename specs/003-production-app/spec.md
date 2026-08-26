# Spec 003: Majlis Production App

## Goal

Deliver Majlis as a complete, production-running Android application backed by an operational .NET API and PostgreSQL database. This specification is the release umbrella for all feature specs; it prevents an individual vertical slice from being treated as the finished product.

Release-wide decisions are normative in `docs/product/v1-product-decisions.md`. Focused behavior is owned by Specs 001, 002, and 004-009.

## Primary User Story

As a Majlis user, I want to install and use the complete app—from account creation through the daily challenge, progress, sharing, friendly comparison, and safe discussion—so that Majlis is a reliable daily cultural experience rather than a demonstration.

## Operator Story

As an authorized content or moderation operator, I want to publish culturally reviewed daily content and handle reports without code or direct database changes so that the live service can operate safely every day.

## Required Capabilities

### Product Experience

- Android onboarding, authentication, profile, preferences, and account recovery.
- Today's Daily Majlis, answer submission, result, explanation, proverb/story context, XP, and streak.
- Attempt history, spoiler-safe sharing, inbound deep links, user-controlled reminders, and a privacy-safe leaderboard.
- Daily discussion, comments, reactions, reporting, and visible moderation outcomes.
- Complete loading, empty, offline, validation, retry, and previously-completed states.

### Content and Operations

- Authenticated content creation, source attribution, editorial review, scheduling, publishing, correction, and unpublishing.
- Authenticated moderation queue and auditable hide, restore, remove, and escalation actions.
- The consent-aware product event catalog and bounded operational telemetry defined by Spec 009.

### Platform

- .NET 10 API, PostgreSQL/EF Core, explicit migrations, and durable user/content/community state.
- Authentication, authorization, input validation, rate limiting, health checks, structured logging, and safe errors.
- Flutter Android client using Riverpod and centralized design, copy, and localization resources.
- Reproducible local environment, deployable backend configuration, backup/restore procedure, and Android release build.

## Release Invariants

- **REL-001**: The backend is authoritative for challenge truth, scoring, streaks, publishing, and moderation state.
- **REL-002**: Correct answers, explanations, and answer-derived data are never exposed before an accepted scored submission.
- **REL-003**: A user receives at most one persisted attempt, XP-ledger award, and streak update per challenge under retries and concurrency.
- **REL-004**: User-generated content is premoderated and reportable; non-visible content never appears directly or indirectly in consumer responses.
- **REL-005**: Private user, attempt, preference, block, report, and operator data is never exposed publicly.
- **REL-006**: Cultural content has a complete Arabic translation, non-empty internal source notes, and an approved immutable revision before publication.
- **REL-007**: One official Daily Majlis is selected by UTC `PublishDate`; V1 profile region/dialect never selects a different edition.
- **REL-008**: No production journey depends on hardcoded mobile content, in-memory-only state, manual database editing, or developer intervention.
- **REL-009**: Private Family Majlis groups and regional editions are post-V1 and shall not be implied by V1 UI/contracts.

## Acceptance Criteria

- **REL-010**: A clean environment can start the database and API using documented commands and apply all committed migrations.
- **REL-011**: A new user can install the Android release build, authenticate, complete today's challenge, receive the correct persisted result/progress, share safely, and participate in moderated discussion.
- **REL-012**: Restarting the API does not lose accounts, content, attempts, streaks, comments, reports, moderation state, editorial revisions, or audit events.
- **REL-013**: Authorized operators can publish the next Daily Majlis and process a report/appeal through supported browser tooling.
- **REL-014**: Unauthorized users cannot access another user's private state or administrative operations.
- **REL-015**: Automated unit, integration, contract, widget, golden, security, and end-to-end tests cover the mapped release paths.
- **REL-016**: Production configuration, secrets, health, telemetry, backup/restore, deployment, and rollback meet every measurable gate in Spec 009.

## Post-V1 Scope

- iOS and web consumer clients.
- Institutional dashboards and course marketplace.
- Paid subscription implementation.
- Advanced narrated audio.
- AI-assisted content generation.
- Broad public social networking outside the moderated daily experience.
