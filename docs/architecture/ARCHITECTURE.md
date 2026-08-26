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
- Auth provider integration.
- Notification integration.
- Storage integration later.

### Majlis.Contracts

- Request/response DTOs shared by API and tests.
- API versioned contracts.

### Majlis.Tests

- Unit tests for domain/application logic.
- Integration tests for API endpoints later.

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
```

State management: Riverpod.

## 6. Data Flow: Daily Challenge

1. App requests `GET /api/v1/daily-majlis/today`.
2. Backend resolves today's published Majlis.
3. Backend returns challenge, options, and content metadata without exposing correct answer.
4. User submits answer to `POST /api/v1/challenges/{challengeId}/attempts`.
5. Backend validates answer and creates attempt.
6. Backend updates XP/streak if first valid completion today.
7. Backend returns result, explanation, streak, and share summary.
8. App displays result and share card.

## 7. Content Publishing Flow

1. Admin creates challenge, story/proverb, and discussion prompt.
2. Admin tags content with region, difficulty, topic, and source notes.
3. Admin marks content as reviewed.
4. Admin schedules Daily Majlis date.
5. System serves published item for that date.

## 8. Moderation Flow

1. User posts comment.
2. Comment is saved as visible or pending depending on policy.
3. User reports comment.
4. Comment receives report count and moderation state.
5. Admin reviews report.
6. Admin hides, restores, or escalates.

## 9. Scalability Notes

The first scale challenge is not compute. It is content operations and moderation. Keep backend simple until retention is proven. Add caching for today's Majlis after usage grows.

## 10. Architectural Decisions

- Daily content is backend-driven.
- Challenge validation is server-side.
- Flutter never receives correct answer before submission.
- Community features require reporting and moderation state.
- Content source notes are internal but mandatory for editorial review.
