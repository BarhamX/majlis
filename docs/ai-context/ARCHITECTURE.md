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

## First Feature Slice

`specs/001-playable-daily-majlis`
