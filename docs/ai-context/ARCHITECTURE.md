# AI Context: Architecture

## Stack

- Flutter Android app.
- .NET 10 LTS ASP.NET Core Web API.
- PostgreSQL.
- EF Core.
- Riverpod in Flutter.

## Backend Structure

```text
src/backend/
  Majlis.Api/
  Majlis.Application/
  Majlis.Domain/
  Majlis.Infrastructure/
  Majlis.Contracts/
  Majlis.Tests/
```

## Mobile Structure

```text
apps/mobile/lib/
  core/
  features/
```

## Core Rule

The backend owns daily challenge truth, answer validation, scoring, streaks, content publishing, and moderation state. Flutter displays and submits; it does not decide correctness.

## First Implementation Slice

`specs/001-playable-daily-majlis`

This slice establishes the daily loop but is not the release boundary. The complete delivery architecture also includes authentication, PostgreSQL persistence, scoring/streaks, Flutter, community safety, content operations, observability, and release infrastructure as defined by `specs/003-production-app/` and the focused specs `004` through `009`.

## Cross-Cutting Decisions

- One global UTC content day and no regional edition selection in V1.
- Arabic launch UI with localized content records and RTL behavior.
- Managed OIDC identity boundary; Majlis stores no passwords.
- Premoderated public comments, user blocking, and auditable moderation.
- Private Family Majlis groups remain post-V1.
