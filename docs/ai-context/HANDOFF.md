# AI Context: Handoff

## Current Status

Majlis remains governed as a complete Production V1 Android application. The Daily Majlis content path is now persisted in PostgreSQL with an explicit migration, database health checking, repeatable local setup, and real PostgreSQL integration coverage; the remaining Production V1 capabilities are still required.

## Latest Task Completed

### 2026-08-26 - PostgreSQL Daily Majlis Persistence

- Added EF Core/Npgsql persistence for `DailyMajlis`, `Challenge`, and `ChallengeOption`.
- Added an explicit initial migration with the documented relationships, audit timestamps, enum storage, and partial unique publish-date index.
- Replaced the in-memory repository with an asynchronous, no-tracking EF repository.
- Added an idempotent Development/Test database initializer and PostgreSQL Compose service.
- Added `/health` with a real `MajlisDbContext` readiness check.
- Added PostgreSQL Testcontainers coverage for retrieval, safe 404 behavior, spoiler safety, health, seed idempotency, and date uniqueness.

### Files Changed

- `.config/dotnet-tools.json` and `compose.yaml`
- `src/backend/Majlis.Api/` - PostgreSQL configuration, startup initialization, health endpoint, and HTTP samples.
- `src/backend/Majlis.Application/` - deterministic option ordering in the response mapper.
- `src/backend/Majlis.Domain/` - EF-compatible private materialization while preserving public validation.
- `src/backend/Majlis.Infrastructure/` - DbContext, mappings, migration, initializer, dependency registration, and EF repository.
- `src/backend/Majlis.Tests/` - PostgreSQL Testcontainers and API integration tests.
- `README.md`, `specs/001-playable-daily-majlis/tasks.md`, `specs/003-production-app/tasks.md`, and this handoff.

### Decisions Made

- `PublishDate` is the canonical UTC calendar date.
- Country-derived UTC offset metadata is deferred until authentication/profile work supplies an ISO country code; the current Daily Majlis API contract remains unchanged.
- The first migration contains only `DailyMajlis`, `Challenges`, and `ChallengeOptions`; unrelated Production V1 tables remain deferred.
- Development and Testing automatically apply committed migrations and idempotently prepare sample content. Production never auto-migrates or seeds.
- Existing scheduled or published content for the current UTC date takes precedence over development seed content.
- `/health` is a database readiness check, not only a process-liveness check.

### Tests and Checks Run

- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj` - expected red phase failed with 4 missing persistence/EF type errors before implementation.
- `docker compose config` - passed.
- `dotnet tool restore` - restored `dotnet-ef` 10.0.11.
- `dotnet tool run dotnet-ef migrations script --idempotent --project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --startup-project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --no-build` - passed and generated the expected three-table script.
- `dotnet tool run dotnet-ef migrations has-pending-model-changes --project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --startup-project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --no-build` - passed with no pending model changes.
- `dotnet format src/backend/Majlis.sln --verify-no-changes --no-restore` - passed.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed with 0 warnings and 0 errors.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-restore` - passed: 9 tests, 0 failed, 0 skipped against PostgreSQL 17.
- `docker compose up -d postgres` and `docker compose exec -T postgres pg_isready -U majlis -d majlis` - PostgreSQL reported healthy and accepted connections.
- Live HTTPS probes returned HTTP 200 and `Healthy` from `/health`, plus the expected spoiler-safe JSON from `/api/v1/daily-majlis/today`.
- `dotnet list src/backend/Majlis.sln package --vulnerable --include-transitive` - no known vulnerable packages found.

### Known Blockers

- None for this slice.
- Docker Desktop must be running for PostgreSQL integration tests.
- Authentication, profiles, attempts, scoring, streaks, and persisted user state remain intentionally deferred.

### Next Recommended Task

Define the focused authentication/profile specification, choose the authentication provider, and implement the User/Profile foundation—including ISO country code—before answer submission, scoring, and streak persistence.

## Previous Task Completed

### 2026-08-26 - Full Production App Scope Alignment

- Replaced the reduced-release framing with a full Production V1 delivery contract.
- Defined the complete user, operator, platform, safety, and release-readiness scope.
- Added a release-wide spec, implementation plan, and completion checklist.
- Reframed the existing Daily Majlis and Community Majlis specs as required implementation slices rather than final delivery boundaries.
- Updated agent instructions and reusable prompts so future tasks remain aligned with the full running app.

### Files Changed

- `AGENTS.md`
- `.github/copilot-instructions.md`
- `.specify/memory/constitution.md`
- `README.md`
- `MANIFEST.md`
- `docs/ai-context/PROJECT.md`
- `docs/ai-context/ARCHITECTURE.md`
- `docs/ai-context/HANDOFF.md`
- `docs/product/PRD.md`
- `docs/product/full-app-scope.md` (replaces the former reduced-scope document)
- `docs/product/roadmap.md`
- `docs/product/acceptance-criteria.md`
- `docs/product/user-stories.md`
- `docs/architecture/TECH_STACK.md`
- `docs/architecture/MODERATION_SAFETY.md`
- `docs/architecture/API_CONTRACTS.md`
- `docs/architecture/DATABASE_SCHEMA.md`
- `docs/business/BRD.md`
- `docs/design/DESIGN.md`
- `docs/prompts/PROMPT_PACK.md`
- `docs/superpowers/plans/2026-08-26-majlis-foundation.md`
- `specs/000-product-foundation/tasks.md`
- `specs/001-playable-daily-majlis/spec.md`
- `specs/001-playable-daily-majlis/plan.md`
- `specs/002-community-majlis/spec.md`
- `specs/003-production-app/spec.md`
- `specs/003-production-app/plan.md`
- `specs/003-production-app/tasks.md`

### Decisions Made

- Production V1 means a complete, installable Android app with an operational .NET/PostgreSQL backend, not merely a successful feature slice.
- The release includes accounts/profile, the complete daily loop, persisted progress, leaderboard, sharing/deep links, reminders, moderated community, content/moderation operations, analytics, and production readiness.
- Internal milestones remain useful for sequencing and testability but cannot redefine project completion.
- iOS/web clients, institutional dashboards, paid products, advanced audio, and AI-assisted content remain post-V1 expansions rather than incomplete Android core functionality.

### Tests and Checks Run

- Repository-wide terminology search confirmed that no document still defines Majlis as a reduced early release.
- `git diff --check -- .specify AGENTS.md MANIFEST.md README.md .github docs specs` - passed; only existing line-ending conversion warnings were reported.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-restore` - failed during a separate, concurrent PostgreSQL red-phase change because its new integration tests referenced packages and persistence types that were not yet available. No backend files were changed by this scope task.

### Known Blockers

- Most Production V1 implementation remains incomplete; use `specs/003-production-app/tasks.md` as the release gate.
- The current PostgreSQL persistence work must be completed and returned to a green build before starting the next backend slice.
- Authentication, hosting, content timezone, Arabic font, first regional focus, and initial comment visibility policy still require decisions.

### Next Recommended Task

Finish and validate the in-progress PostgreSQL persistence slice, including migrations, health checks, idempotent initialization, and PostgreSQL-backed integration tests. Then implement authentication/profile and the persisted attempt/scoring/streak slice in the order defined by `specs/003-production-app/plan.md`.

## Earlier Task Completed

### 2026-08-26 - Initial Playable Daily Majlis Backend

- Created the backend solution and all six planned projects.
- Added `DailyMajlis`, `Challenge`, and `ChallengeOption` domain entities.
- Enforced exactly one correct option for a multiple-choice challenge.
- Added the API response DTOs and the `IDailyMajlisService` application contract.
- Implemented the date-aware Daily Majlis query and an in-memory seed repository.
- Added the versioned endpoint and replaced the template weather endpoint.
- Added application and domain unit tests.

### Files Changed

- `src/backend/Majlis.sln`
- `src/backend/Majlis.Api/` - API project, composition root, controller, host settings, and HTTP request sample.
- `src/backend/Majlis.Application/` - today's query service and repository/service abstractions.
- `src/backend/Majlis.Contracts/` - spoiler-safe Daily Majlis response contracts.
- `src/backend/Majlis.Domain/` - Daily Majlis aggregate, challenge, option, and supporting statuses.
- `src/backend/Majlis.Infrastructure/` - temporary in-memory seed repository.
- `src/backend/Majlis.Tests/` - application service and domain invariant tests.
- `specs/001-playable-daily-majlis/tasks.md`
- `docs/ai-context/HANDOFF.md`

### Decisions Made

- Used the canonical versioned route `/api/v1/daily-majlis/today` from the conventions and API contract.
- Kept the correct option, explanation, source notes, and editorial state in the Domain model; the pre-attempt response DTO exposes only option ids and text.
- Placed the temporary seeded repository in Infrastructure so it can be replaced without changing the controller or application contract.
- Used an injected `TimeProvider` and a UTC date boundary for deterministic tests. The product scheduling timezone remains to be decided before database-backed publishing.
- Returned a safe `404` problem response when no published Daily Majlis is available.
- Kept `userState` at `hasAttempted: false` and `currentStreak: 0` until authentication and attempt persistence are implemented.

### Tests and Checks Run

- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj` - expected red phase failed with 5 missing-type compiler errors before implementation.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed with 0 warnings and 0 errors.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-build` - passed: 2 tests, 0 failed, 0 skipped.
- `dotnet run --project src/backend/Majlis.Api/Majlis.Api.csproj --configuration Release --no-build --no-launch-profile --urls https://127.0.0.1:5092` plus `curl.exe --insecure --fail-with-body --silent --show-error --write-out "`nHTTP %{http_code}`n" https://127.0.0.1:5092/api/v1/daily-majlis/today` - returned HTTP 200 with the expected JSON and no correct-answer field.

### Known Blockers

- None for this slice.
- PostgreSQL/EF Core, authentication, attempts, scoring, and persisted user state were outside that initial slice and remain required for Production V1.
- The final content scheduling timezone is not yet specified; the placeholder query currently uses UTC.

### Next Recommended Task

Add the health endpoint and PostgreSQL/EF Core configuration, create the first explicit migration for Daily Majlis content, replace the seed repository with persistence, and add an API integration test for the spoiler-safe response.

## Open Decisions

- Authentication provider: custom JWT vs managed identity provider.
- Hosting provider.
- Final Arabic font family.
- First regional content focus: Qatar/Gulf, pan-Arab, or mixed.
- Whether comments are visible immediately or pending review during beta.

## Last Updated

2026-08-26
