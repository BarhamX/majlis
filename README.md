# Majlis

Majlis is a daily Arab culture challenge game for Android, built with Flutter and a .NET backend. It recreates the spirit of a traditional majlis as a modern mobile ritual: short cultural challenges, proverbs, stories, discussion prompts, family/friend competition, and shareable results.

## Product Positioning

**Majlis is Wordle-style daily play for Arab cultural knowledge.**

The app does not shame users for what they do not know. It playfully provokes curiosity:

> Today's Majlis is open. Can you answer before your friends?

## Core Loop

1. User opens today's Majlis.
2. User answers a short cultural challenge.
3. App reveals answer, meaning, and short context.
4. User sees streak, XP, and comparison with friends/family.
5. User contributes to the daily discussion.
6. User shares a result or proverb card.

## Repository Status

Majlis is under active implementation as a complete production Android application. The repository contains the product, business, UX, design, architecture, agent context, and specifications, plus the first .NET backend slice. The Flutter client and the remaining production capabilities are still to be implemented.

## Important Directories

```text
.specify/memory/constitution.md      Spec Kit project constitution
specs/                              Feature specs, plans, and tasks
AGENTS.md                           Agent operating instructions
docs/product/                       PRD, personas, journeys, full-app scope, roadmap
docs/business/                      BRD and business model
docs/design/                        UX, theme, visual identity, content voice
docs/architecture/                  System architecture, API, database, stack
docs/ai-context/                    Files Codex/agents must read first
docs/prompts/                       Reusable prompt pack
apps/mobile/                        Flutter Android application (implementation pending)
src/backend/                        Active .NET backend solution
```

## Recommended Implementation Order

1. Complete PostgreSQL persistence, migrations, health, and integration-test infrastructure.
2. Implement authentication, authorization, profile, and preferences.
3. Complete challenge submission, scoring, streaks, history, and share contracts.
4. Build the complete Flutter Android experience.
5. Add leaderboard, native sharing/deep links, and user-controlled reminders.
6. Implement community responses, reporting, and moderation.
7. Implement content and moderation administration.
8. Add analytics, observability, security hardening, deployment, and release validation.

## Delivery Target

**Majlis Production V1** is a complete, running Android app backed by the production-ready .NET API and PostgreSQL database. It includes accounts, the daily cultural loop, scoring and streaks, sharing, leaderboards, moderated discussion, content administration, analytics, and release operations.

Implementation remains milestone-based so every slice is testable. **M1: Playable Daily Majlis** is the first vertical slice, not the final product boundary. See `docs/product/full-app-scope.md` and `specs/003-production-app/` for the release definition.

## Backend Local Development

The backend requires .NET 10 and Docker Desktop.

```powershell
docker compose up -d postgres
dotnet tool restore
dotnet run --project src/backend/Majlis.Api/Majlis.Api.csproj
```

Development startup applies committed migrations and idempotently prepares the sample Daily Majlis for the current UTC date. Production does not migrate or seed automatically and must supply `ConnectionStrings__MajlisDatabase`.

The local endpoints are:

- `GET http://localhost:5129/health`
- `GET http://localhost:5129/api/v1/daily-majlis/today`

Stop the local database without deleting its named volume:

```powershell
docker compose down
```
