# Backend Architecture

The Majlis backend uses clean architecture to keep cultural game rules independent from HTTP hosting, persistence, and infrastructure details.

## Architecture Goal

The backend must make the Daily Majlis game loop reliable, testable, and safe:

1. Select the correct daily experience.
2. Serve spoiler-safe challenge content.
3. Validate answers server-side.
4. Track attempts, score, streaks, and progress.
5. Support future content management and moderation.

## Layer Responsibilities

### Majlis.Domain

Owns the core business concepts.

Examples:

- Daily Majlis
- Challenge
- Challenge option
- Proverb
- Story
- Difficulty
- Region/dialect tags
- Streak rules
- Attempt rules

Rules:

- No database code.
- No HTTP code.
- No framework-specific business logic.
- Keep entities valid by construction where practical.

### Majlis.Application

Owns use cases and orchestration.

Examples:

- Get today's Daily Majlis
- Submit challenge answer
- Calculate answer result
- Update streak
- Return discussion feed

Rules:

- Depends on Domain and Contracts.
- Defines interfaces for repositories/services needed from Infrastructure.
- Keeps endpoints thin.

### Majlis.Contracts

Owns API-facing request and response models.

Examples:

- `DailyMajlisTodayResponse`
- `ChallengeResponse`
- `SubmitAttemptRequest`
- `SubmitAttemptResponse`

Rules:

- Do not expose domain internals directly.
- Keep response models stable for Flutter.
- Use explicit DTO names.

### Majlis.Infrastructure

Owns technical adapters.

Examples:

- In-memory repositories
- EF Core repositories
- PostgreSQL persistence
- Content seed loading
- Clock/date providers
- External file/object storage later

Rules:

- Implements abstractions defined by Application/Domain.
- No HTTP endpoint definitions.
- No mobile-specific assumptions.

### Majlis.Api

Owns hosting and HTTP boundary.

Examples:

- Endpoint mapping
- Dependency injection
- OpenAPI configuration
- Auth middleware later
- Error handling middleware

Rules:

- No game logic in controllers/endpoints.
- Map HTTP requests to Application services.
- Return typed contracts.

## Dependency Direction

```text
Majlis.Api
  -> Majlis.Application
  -> Majlis.Domain
  -> Majlis.Contracts

Majlis.Infrastructure
  -> Majlis.Application
  -> Majlis.Domain

Majlis.Tests
  -> all test targets as needed
```

Domain should remain the most stable layer.

## Daily Majlis Flow

```text
Flutter client
  -> GET /api/v1/daily-majlis/today
  -> Majlis.Api endpoint
  -> Application service
  -> Repository/date provider
  -> Domain model
  -> Spoiler-safe contract
  -> Flutter renders daily experience
```

Answer submission will later follow:

```text
Flutter selected option
  -> POST /attempts
  -> Application validates attempt
  -> Domain determines correctness
  -> Score/streak service updates progress
  -> Result contract returned to Flutter
```

## Cultural Safety Boundary

Cultural content quality is a product and trust concern, not just a database concern. The architecture must support:

- Source attribution
- Region/dialect tagging
- Content review status
- Moderator review for user-generated responses
- Avoiding regional, tribal, sectarian, or ethnic insult patterns

## Future Persistence Direction

When persistence is added:

- Use PostgreSQL.
- Add EF Core in `Majlis.Infrastructure`.
- Keep migrations explicit.
- Keep seed data separate from runtime user-generated content.
- Store daily content schedule server-side.

## Future Authentication Direction

Authentication is not part of the first backend scaffold. When added:

- Keep anonymous read access optional for today's Majlis if product decides.
- Require authentication for attempts, streaks, groups, comments, reports, and profile.
- Do not trust client-side identity or score data.
