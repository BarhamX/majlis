# Majlis

Majlis is a daily Arab culture challenge game for Android, built with Flutter and a .NET backend. It recreates the spirit of a traditional majlis as a modern mobile ritual: short cultural challenges, proverbs, stories, discussion prompts, external family/friend sharing, an opt-in global leaderboard, and shareable results.

## Product Positioning

**Majlis is Wordle-style daily play for Arab cultural knowledge.**

The app does not shame users for what they do not know. It playfully provokes curiosity:

> Today's Majlis is open. Can you answer before your friends?

## Core Loop

1. User opens today's Majlis.
2. User answers a short cultural challenge.
3. App reveals answer, meaning, and short context.
4. User sees streak, XP, and—when eligible and opted in—the global weekly leaderboard.
5. User contributes to the daily discussion.
6. User shares a result or proverb card.

## Repository Status

Majlis is under active implementation as a complete production Android application. The repository contains the product, business, UX, design, architecture, agent context, and specifications, plus PostgreSQL-backed Daily Majlis retrieval and the local identity/profile backend foundation. The Flutter client and the remaining production capabilities are still to be implemented.

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
docs/quality/                       Requirement-to-test traceability
apps/mobile/                        Flutter Android application (implementation pending)
src/backend/                        Active .NET backend solution
```

## Recommended Implementation Order

1. Complete PostgreSQL persistence, migrations, health, and integration-test infrastructure.
2. Implement the Spec 004 local identity/profile foundation using the Development/Testing issuer; production login providers are Google, Apple, Meta, and Snapchat.
3. Complete challenge submission, scoring, streaks, history, and share contracts.
4. Build the complete Flutter Android experience.
5. Reach the internal `Game Ready` milestone with the persisted Arabic/RTL daily loop.
6. Implement Specs 005-007 for leaderboard, native sharing/deep links, and user-controlled reminders.
7. Implement community responses, reporting, and moderation.
8. Implement Spec 008 content and moderation administration.
9. After `Game Ready`, configure Google/Apple/Meta/Snapchat production identity, hosting, domains, verified App Links, signing, staging, and Spec 009 release operations; this can proceed alongside the remaining local product work.

## Delivery Target

**Majlis Production V1** is a complete, running Android app backed by the production-ready .NET API and PostgreSQL database. It includes accounts, the daily cultural loop, scoring and streaks, sharing, leaderboards, moderated discussion, content administration, analytics, and release operations.

Implementation remains milestone-based so every slice is testable. **M1: Playable Daily Majlis** is the first vertical slice, not the final product boundary. See `docs/product/full-app-scope.md` and `specs/003-production-app/` for the release definition.

Production V1 decisions are fixed in `docs/product/v1-product-decisions.md`; every normative requirement is mapped in `docs/quality/requirements-to-tests.md`.

## Backend Local Development

The backend requires .NET 10 and Docker Desktop.

```powershell
docker compose up -d postgres
dotnet tool restore
dotnet run --project src/backend/Majlis.Api/Majlis.Api.csproj --launch-profile https
```

Development startup applies committed migrations and idempotently prepares the sample Daily Majlis for the current UTC date. Production does not migrate or seed automatically and must supply `ConnectionStrings__MajlisDatabase`.

Development uses an ephemeral signed test issuer; its signing key is generated in memory and the mode is rejected outside Development/Testing. Obtain a local bearer token from `POST /api/v1/dev/auth/token`, then call `POST /api/v1/me/bootstrap`. Production Google/Apple/Meta/Snapchat credentials and callbacks are intentionally not configured before `Game Ready`.

`dotnet test src/backend/Majlis.sln` starts an isolated PostgreSQL Testcontainer, so Docker Desktop must be running.

The local endpoints are:

- `GET https://localhost:7204/health`
- `POST https://localhost:7204/api/v1/dev/auth/token` (Development/Testing only)
- `POST https://localhost:7204/api/v1/me/bootstrap`
- `GET/PUT https://localhost:7204/api/v1/me/profile`
- `GET https://localhost:7204/api/v1/daily-majlis/today`

Stop the local database without deleting its named volume:

```powershell
docker compose down
```

## Documentation Validation

Run the specification/link/manifest checks directly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1
```

The current checkout uses the repository hook in `.githooks/pre-commit`. A fresh clone can enable it with:

```powershell
git config core.hooksPath .githooks
```
