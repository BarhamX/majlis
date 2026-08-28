# Majlis System Architecture

## 1. Overview

Majlis uses a Flutter Android mobile client and a .NET Web API backend. The backend owns identity, daily content scheduling, challenge validation, scoring, streaks, discussions, moderation, and admin content operations.

## 2. High-Level Components

```text
Flutter Android App
  | HTTPS JSON API
ASP.NET Core API
  | Application services
Domain model
  | Repository interfaces
Infrastructure
  | EF Core
PostgreSQL
```

## 3. Backend Layers

### Majlis.Api

- Controllers/minimal API endpoints.
- Auth middleware.
- Request validation boundary.
- OpenAPI documentation.

### Majlis.Application

- Use cases.
- Commands/queries.
- DTO mapping.
- Scoring and streak orchestration.
- Moderation workflows.

### Majlis.Domain

- Entities and value objects.
- Domain rules for attempts, streaks, content status, moderation status.
- No dependency on infrastructure.

### Majlis.Infrastructure

- EF Core DbContext.
- Repositories.
- Google, Apple, Meta, and Snapchat validation adapters behind one identity boundary, plus a Development/Testing-only signed test issuer.
- Android reminder preferences and analytics delivery integration.
- Storage integration only when a specified feature requires server-owned media.

### Majlis.Contracts

- Request/response DTOs shared by API and tests.
- API versioned contracts.

### Majlis.Tests

- Unit tests for domain/application logic.
- PostgreSQL-backed integration tests for API authorization, persistence, uniqueness, idempotency, and response safety from the first endpoint implementation.

## 4. Core Domain Areas

- Users and Profiles
- Daily Majlis
- Challenges and Options
- Attempts and Scoring
- Streaks and XP
- Proverbs and Stories
- Discussion and Reactions
- Reports and Moderation
- Admin Content Workflow
- Analytics Events
- Identity, Consent, and Deletion
- Localized Content Revisions
- Leaderboard Projection
- User Blocking and Appeals
- Immutable Audit Events

## 5. Mobile Architecture

Flutter app is feature-first:

```text
lib/
  core/
    api/
    auth/
    routing/
    theme/
    localization/
    widgets/
  features/
    onboarding/
    daily_majlis/
    challenge/
    results/
    discussion/
    profile/
    leaderboard/
    sharing/
    reminders/
```

State management: Riverpod.

## 6. Data Flow: Daily Challenge

1. App requests `GET /api/v1/daily-majlis/today`.
2. Backend resolves the one publication for today's UTC date and the requested locale.
3. Backend returns challenge, options, and content metadata without correct answer, explanation, sources, or review state.
4. User submits one final answer with an idempotency key to `POST /api/v1/challenges/{challengeId}/attempts`.
5. Backend atomically creates the attempt, 10/15 XP ledger entry, and UTC content-day streak mutation.
6. Duplicate and concurrent submissions converge on the original attempt without another award.
7. Backend returns result, explanation, progress, and content revision.
8. App displays result and share card.

## 7. Content Publishing Flow

1. Admin creates challenge, story/proverb, and discussion prompt.
2. Admin provides complete Arabic text, region/dialect provenance tags, difficulty, topic, and required source notes in an immutable draft revision.
3. A different reviewer approves the revision.
4. A publisher schedules the revision for one UTC date.
5. An idempotent scheduler publishes it at `00:00:00Z` and the API serves that revision globally.

## 8. Moderation Flow

1. User posts or edits one response after completing the Daily Majlis.
2. The revision is pending and visible only to its author and moderators.
3. A moderator approves it for public visibility.
4. A user may report or block from a visible response.
5. A moderator reviews the prioritized report and records an audited action.
6. An eligible user may appeal; a different moderator decides the appeal.

## 9. Scalability Notes

The first scale challenge is not compute. It is content operations and moderation. Keep backend simple until retention is proven. Add caching for today's Majlis after usage grows.

## 10. Architectural Decisions

- Daily content is backend-driven.
- Challenge validation is server-side.
- Flutter never receives correct answer before submission.
- Community features require reporting and moderation state.
- Content source notes are internal but mandatory for editorial review.
- Arabic is the required launch locale and localized content is stored in translation tables.
- Private Family Majlis and region-specific editions are post-V1.
- External providers own account authentication and recovery; Majlis owns identity linking, local authorization, and privacy state and never auto-links by email.
- Local Android scheduling owns V1 reminders; no marketing push channel is required.

## Delivery Boundary Before Logistics

Feature development uses local PostgreSQL, configurable URLs, and a deterministic test identity issuer until the `Game Ready` milestone in `V1-DEC-011`. Google, Apple, Meta, and Snapchat credentials/callbacks, hosting, public domains, verified App Links, signing, staging, backup, and deployment remain mandatory for Production V1 but begin after that milestone.
