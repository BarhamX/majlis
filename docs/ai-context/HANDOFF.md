# AI Context: Handoff

## Current Status

Majlis remains governed as a complete Production V1 Android application. The release documentation now closes the prior product contradictions, defines every major V1 feature and target contract, and maps 147 normative requirements to planned verification. Google Account and Sign in with Apple are the selected Production V1 identities, while hosting/domain/provider credential logistics are intentionally sequenced after the local `Game Ready` milestone. The implemented backend still contains only the persisted read-only Daily Majlis slice; the focused implementation tasks remain required.

## Latest Task Completed

### 2026-08-26 - Production V1 Specification Hardening

- Reconciled family scope, UTC publishing, regional semantics, Arabic launch behavior, comments, gameplay, privacy, and retention in one normative V1 decision register.
- Added focused spec/plan/task triplets for authentication/profile, leaderboard, sharing/deep links, reminders, content/moderation administration, and production operations.
- Defined exact scoring, retries, streak boundaries, idempotency/concurrency, comment visibility, blocking, appeals, age safeguards, and account deletion behavior.
- Replaced the draft API and database documents with complete V1 target contracts, including localized immutable content revisions and required source notes.
- Added measurable performance, Android-version, accessibility, reliability, security, analytics, backup/restore, deployment, and release gates.
- Added requirement-to-test traceability and a repository-local validation hook.
- Archived the stale foundation work plan and made the reusable feature prompt select its focused spec instead of hardcoding Spec 001.
- Fixed Google Account and Sign in with Apple as the only V1 identity choices and deferred production identity/hosting/domain logistics until after the local `Game Ready` milestone.

### Files Changed

- Product/business/design: `docs/product/`, `docs/business/`, and `docs/design/`.
- Architecture/contracts: `docs/architecture/` and `docs/ai-context/`.
- Specifications: hardened Specs 001-003 and added Specs 004-009.
- Quality/tooling: `docs/quality/requirements-to-tests.md`, `scripts/validate-docs.ps1`, `.githooks/pre-commit`, and `.gitattributes`.
- Repository guidance/inventory: `AGENTS.md`, `.github/copilot-instructions.md`, `README.md`, `MANIFEST.md`, `docs/prompts/PROMPT_PACK.md`, and the archived foundation plan.
- No backend or Flutter implementation file was changed.

### Decisions Made

- One global Daily Majlis uses a UTC `PublishDate`; Qatar/Gulf is the initial editorial focus, not a segmented edition.
- Arabic is the required launch locale, with Noto Sans Arabic, RTL-first UI, BCP 47 negotiation, and localized content records.
- V1 has external family/friend sharing but no private Family Majlis, private discussion, or family leaderboard.
- Google Account and Sign in with Apple own account authentication/recovery; Majlis owns explicit identity linking, local users, roles, privacy, and deletion state and never merges by email.
- One final attempt awards 10 completion XP plus 5 correct-answer XP; both outcomes advance a streak across eligible published UTC days.
- Public comments are premoderated. Blocking, appeals, minor safeguards, deletion, and retention are explicit.
- The V1 leaderboard is adult-only, opt-in, global, and weekly; reminders are local Android notifications and off by default.
- `Game Ready` requires the persisted Arabic/RTL daily journey against Development/Testing identity; production credentials, hosting, domains, App Links, signing, staging, and deployment follow it without leaving the Production V1 release scope.

### Tests and Checks Run

- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1` - passed: 60 Markdown files and 147 requirement ids.
- `git hook run pre-commit` - passed with the same documentation checks.
- `git diff --check` - passed; only expected Git line-ending conversion warnings were printed.
- Runtime backend/Flutter tests were not run because this task changed documentation and repository validation only.

### Known Blockers

- The current migration/domain still allow nullable source notes and non-localized content; the target schema explicitly requires a forward migration rather than editing the applied migration.
- All implementation and evidence rows marked `Planned` in the traceability matrix remain release work.
- Google/Apple production credentials and verification, Apple redirect/service configuration, hosting, canonical public hosts, signing fingerprints, staging, and deployment are deliberately deferred until `Game Ready`; they remain release gates.
- Legal/product review must confirm enabled launch jurisdictions for 13-17 accounts and the documented retention windows before production launch.

### Next Recommended Task

Implement the provider-neutral portion of Spec 004 test-first: add signed-test-token infrastructure, local user/identity/profile/consent/deletion persistence, and cross-user authorization tests without production Google/Apple credentials or hosting dependencies.

## Previous Work

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

At this point in history, authentication/profile specification and provider choice were still open. The later hardening decision selects Google and Apple and moves their production configuration after `Game Ready`.

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
- At this point in history, authentication, hosting, content timezone, Arabic font, regional focus, and comment visibility were open; the later Production V1 Specification Hardening entry resolves the product policies and leaves only the listed implementation gates.

### Next Recommended Task

Finish and validate the in-progress PostgreSQL persistence slice, including migrations, health checks, idempotent initialization, and PostgreSQL-backed integration tests. Then implement authentication/profile and the persisted attempt/scoring/streak slice in the order defined by `specs/003-production-app/plan.md`.

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
- Used an injected `TimeProvider` and a UTC date boundary for deterministic tests. The later persistence and specification tasks made UTC the canonical content day.
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
- At this point in history, content scheduling was unresolved; it is now fixed to the UTC content day.

### Next Recommended Task

Add the health endpoint and PostgreSQL/EF Core configuration, create the first explicit migration for Daily Majlis content, replace the seed repository with persistence, and add an API integration test for the spoiler-safe response.

## Implementation Gates

- After `Game Ready`, configure Google Account and Sign in with Apple production credentials, including the Apple service/redirect configuration, and verify both in staging.
- After `Game Ready`, select the hosting/managed PostgreSQL providers and record the Spec 009 RPO/RTO and data-residency evidence.
- After `Game Ready`, configure the canonical production/staging hosts and Android signing fingerprints for verified App Links.
- Confirm launch jurisdictions and obtain legal/product approval for minor accounts and retention.
- Name the product, editorial, security/privacy, engineering, and operations release approvers.

## Last Updated

2026-08-26
