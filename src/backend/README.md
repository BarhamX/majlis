# Majlis Backend

The Majlis backend is the .NET API layer for the daily Arab cultural challenge game. It owns daily content delivery, challenge rules, spoiler-safe result handling, streaks, scoring, moderation hooks, and future persistence.

## Current Scope

The current backend slice supports the first playable product milestone:

- Return today's Daily Majlis payload.
- Keep challenge answers spoiler-safe before the user submits.
- Expose a versioned API endpoint for the mobile app.
- Use clean architecture boundaries so business rules are not implemented in controllers.

## Solution Layout

```text
src/backend/
  Majlis.Api/              HTTP API, dependency injection, endpoints, hosting
  Majlis.Application/      Use cases, application services, orchestration
  Majlis.Contracts/        API request/response DTOs shared at boundaries
  Majlis.Domain/           Core entities, value objects, enums, business rules
  Majlis.Infrastructure/   Repositories, seed data, persistence adapters
  Majlis.Tests/            Unit and application-level tests
  Majlis.sln               Backend solution
```

## Architectural Rules

- `Majlis.Domain` must not depend on API, Infrastructure, or Application.
- `Majlis.Application` may depend on Domain and Contracts.
- `Majlis.Infrastructure` implements Application/Domain abstractions.
- `Majlis.Api` wires dependencies and exposes endpoints only.
- Controllers/endpoints should not contain cultural game logic.
- API responses must not expose correct answers before answer submission.

## Local Development

From repository root:

```powershell
cd src/backend
dotnet restore Majlis.sln
dotnet build Majlis.sln
dotnet test Majlis.sln
```

Run the API:

```powershell
cd src/backend/Majlis.Api
dotnet run
```

## Current Endpoint

```http
GET /api/v1/daily-majlis/today
```

Purpose: returns the active Daily Majlis experience for the current date.

The response is intended for the Flutter client to render the daily ritual screen, challenge prompt, proverb/story preview, and discussion prompt.

## Required Before Each Backend Task

Read these files first:

1. `AGENTS.md`
2. `docs/ai-context/PROJECT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/CONVENTIONS.md`
5. `docs/ai-context/HANDOFF.md`
6. `.specify/memory/constitution.md`
7. Relevant files in `specs/`

## Handoff Rule

Every backend change must update:

```text
docs/ai-context/HANDOFF.md
```

Include:

- Task completed
- Files changed
- Decisions made
- Commands run
- Test results
- Blockers
- Next recommended step
