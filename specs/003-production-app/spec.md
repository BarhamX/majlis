# Spec 003: Majlis Production App

## Goal

Deliver Majlis as a complete, production-running Android application backed by an operational .NET API and PostgreSQL database. This specification is the release umbrella for all feature specs; it prevents an individual vertical slice from being treated as the finished product.

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
- Basic product and operational analytics.

### Platform

- .NET 10 API, PostgreSQL/EF Core, explicit migrations, and durable user/content/community state.
- Authentication, authorization, input validation, rate limiting, health checks, structured logging, and safe errors.
- Flutter Android client using Riverpod and centralized design, copy, and localization resources.
- Reproducible local environment, deployable backend configuration, backup/restore procedure, and Android release build.

## Release Invariants

- The backend is authoritative for challenge truth, scoring, streaks, publishing, and moderation state.
- Correct answers are never exposed before a scored submission.
- A user receives at most one score/XP/streak update per challenge.
- User-generated content is reportable and hidden content never appears in normal public queries.
- Private user, attempt, and group data is never exposed publicly.
- Cultural content has source notes and editorial status before publication.
- No production journey depends on hardcoded mobile content, in-memory-only state, manual database editing, or developer intervention.

## Acceptance Criteria

- A clean environment can start the database and API using documented commands and apply all committed migrations.
- A new user can install the Android release build, authenticate, complete today's challenge, receive the correct persisted result/progress, share safely, and participate in moderated discussion.
- Restarting the API does not lose accounts, content, attempts, streaks, comments, reports, or moderation state.
- An authorized operator can publish the next Daily Majlis and process a report through supported tooling.
- Unauthorized users cannot access another user's private state or administrative operations.
- Automated unit, integration, contract, widget, and end-to-end tests cover the critical release paths.
- Production configuration, secrets handling, health monitoring, logging, backup/restore, and release/rollback procedures are documented and verified.

## Post-V1 Scope

- iOS and web consumer clients.
- Institutional dashboards and course marketplace.
- Paid subscription implementation.
- Advanced narrated audio.
- AI-assisted content generation.
- Broad public social networking outside the moderated daily experience.
