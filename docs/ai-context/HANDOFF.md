# AI Context: Handoff

## Current Status

Majlis remains governed as a complete Production V1 Android application. The backend now has the local identity/profile foundation needed for game development plus a persisted, localized Daily Majlis checkpoint with immutable published revisions, history-safe UTC rollover, scheduled-content precedence, and concurrency-safe Development/Testing initialization. Google, Apple, Meta, and Snapchat are the selected production providers, but their credentials/callbacks and all hosting/domain logistics remain deferred until `Game Ready`.

## Work in Progress

### 2026-08-28 - Task 4 Review Fix Round 2

- Diagnosed hosted PostgreSQL run `33173237820` (114 passed, 4 failed): an unpublished app-created seed retained its immutable publication fact but was not recognized by the fixed-ID-only repair lookup, while three result tests expected text that does not exist in the real PostgreSQL seed.
- The initializer now reuses the Daily Majlis that owns today's immutable publication fact and concurrent repair converges through the revision-number constraint. Unique-violation handling is restricted to the exact create/repair race constraints so unrelated PostgreSQL 23505 failures propagate.
- PostgreSQL replay, restart, and correction tests now compare the first accepted stored result with the later result and independently assert the real seeded English locale/explanation plus `Content-Language`; the correction case also rejects substituted corrected Arabic content.
- Local focused/non-integration tests and Release compilation pass. PostgreSQL integration execution remains hosted-only because local Docker/PostgreSQL is unavailable; the controller must rerun hosted CI.

### Files Changed

- `DailyMajlisDatabaseInitializer.cs`, new `DailyMajlisInitializationConflict.cs`, Infrastructure test visibility, focused conflict tests, affected PostgreSQL tests, `MANIFEST.md`, and this handoff.

### Known Blockers

- Hosted PostgreSQL rerun is required to validate the repaired initializer race and corrected durable-result assertions.

### Next Recommended Task

- Rerun hosted Backend CI for this fix commit; continue Task 4 review only after the full PostgreSQL suite is green.

### 2026-08-28 - Task 4 Review Fix Round 1

- Added immutable `DailyMajlisPublications` history and a new forward migration with deterministic legacy `published`/`unpublished` backfill; streak eligibility no longer depends on mutable publication status.
- Moved submission time/day capture after the authenticated-user row lock, revalidated token issuance against the locked `AuthenticationNotBefore`, and added a PostgreSQL `FOR SHARE` locking read for the current publication decision.
- Completed daily-loop problem envelopes with stable `type`, `title`, `status`, `code`, and `traceId`, without error details; reverted DLY-005, ATT-006, and ATT-007 to `Planned` pending hosted PostgreSQL execution.
- Added locally executable regression tests plus compiled PostgreSQL coverage for published-then-unpublished streak history, midnight waiting, unpublishing races, and fresh/upgrade migration backfill. PostgreSQL remains unexecuted locally because Docker Desktop/PostgreSQL is unavailable.

### Files Changed

- Daily Majlis domain/configuration/context/initializer, `EfDailyLoopRepository`, `DailyLoopService`, daily-loop controllers/problem results, new publication-history migration/snapshot, focused tests, schema/traceability/manifest, and this handoff.

### Tests and Checks Run

- See the Task 4 fix-round report for exact RED/GREEN and final verification commands. Local PostgreSQL integration execution remains deferred to hosted CI.

### Known Blockers

- Hosted PostgreSQL execution is still required for migration backfill and transaction-lock race validation; no local integration success is claimed.

### Next Recommended Task

- Re-review this fix commit, then run hosted Backend CI before Task 5 rate-limit/security work.

### 2026-08-28 - Transactional Daily-Loop Application and APIs

- Implemented Task 4 of the persisted backend daily loop test-first: application-owned submission orchestration, one EF/PostgreSQL transaction with authenticated-user locking/revalidation, deterministic idempotency, immutable result mapping, owned result/history/progress/share queries, thin API controllers, and real Today completion state.
- Authored PostgreSQL coverage for current-day/option ownership, exact awards and streaks, database-trigger rollback, replay/conflicts, restart durability, correction/unpublishing snapshots, ownership, history, share safety, and same/different-key races. These tests compile but were not run locally because Docker Desktop/PostgreSQL is unavailable; hosted CI execution remains required before upgrading their evidence beyond `Partial`.

### Files Changed

- `src/backend/Majlis.Application/DailyLoop/` - use-case contracts, orchestration, stable exceptions, cursor codec, request hashing, result mapping, and configurable share-link settings.
- `src/backend/Majlis.Infrastructure/DailyLoop/EfDailyLoopRepository.cs` - transaction/retry boundary, user-row lock, EF persistence/query implementation, keyset history, and PostgreSQL conflict translation.
- `src/backend/Majlis.Contracts/DailyLoop/` and `src/backend/Majlis.Api/Controllers/` - submit/result/history/progress/share contracts and thin authenticated endpoints.
- `DailyMajlisController.cs`, `Program.cs`, Infrastructure dependency registration, and application settings - Today attempt state, daily-loop composition, and configurable share host.
- `src/backend/Majlis.Tests/Application/DailyLoopServiceTests.cs` and `Integration/DailyLoopPostgreSqlTests.cs` - application GREEN coverage plus compiled PostgreSQL scenarios.
- `docs/quality/requirements-to-tests.md`, `MANIFEST.md`, and `docs/ai-context/HANDOFF.md` - partial evidence, file inventory, and this handoff; Spec 001 task checkboxes remain open pending hosted PostgreSQL execution and Task 5 security/rate-limit work.

### Decisions Made

- Kept orchestration in Application through an explicit transaction/repository port; Infrastructure owns EF, PostgreSQL `FOR UPDATE`, transaction retry, unique/concurrency detection, and opaque keyset query details.
- Scoped attempt idempotency to `challenge_attempt` and hashed canonical challenge/selected-option UUIDs. Same key/payload rebuilds the original response from stored attempt/revision data; changed payload and different-key completion remain distinct stable conflicts.
- Cleared the request-scoped EF tracker before starting the mutation transaction so the locked user row and completed-profile status are reloaded and revalidated inside the award transaction rather than reused from authorization middleware state.
- Kept absent progress implicit: `/me/progress` returns zeros without creating `UserProgress`; the first accepted attempt creates it in the same transaction as attempt, ledger, snapshots, and idempotency.
- Used the stored `ContentRevisionId`, `ResultLocale`, and post-award XP/streak snapshots for all replays and owned reads. The configured share host only produces spoiler-safe metadata; no rendering, landing route, or deep-link behavior was added.

### Tests and Checks Run

- RED: `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~DailyLoopServiceTests"` failed with expected CS0234/CS0246 errors because the `Majlis.Application.DailyLoop` use-case types did not exist.
- GREEN: the same focused command passed 11 tests, 0 failed after the minimal Application/Infrastructure/API implementation.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-restore --filter "FullyQualifiedName!~Majlis.Tests.Integration"` - passed: 57 tests, 0 failed, 0 skipped.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed with 0 warnings and 0 errors.
- Scoped `dotnet format --verify-no-changes` for all Task 4 C# files passed.
- Repository-pinned `dotnet-ef` 10.0.11 `migrations has-pending-model-changes` passed with no model drift.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1` - passed: 71 Markdown files and 147 requirement ids.
- `git diff --check` passed; only expected line-ending conversion warnings were printed.

### Known Blockers

- `DailyLoopPostgreSqlTests` are authored and compile but were not executed locally: the task began with Docker Desktop/PostgreSQL unavailable, and local integration execution was explicitly excluded. Hosted CI must validate runtime SQL locking, rollback, restart, cursor, correction, and race behavior.
- Attempt-rate limiting, rejected-request no-mutation proof, the remaining malformed-header/security matrix, and full PostgreSQL confirmation are Task 5. No Spec 001 task checkbox or evidence row was marked verified in this checkpoint.

### Next Recommended Task

Run this commit in hosted Backend CI. If the PostgreSQL suite is green, complete Task 5 test-first: account/IP attempt limits plus the remaining authorization, malformed-input, cursor-stability, response-allowlist, and concurrency/security verification.

### 2026-08-28 - Daily-Loop Domain and Persistence Foundation

- Implemented Task 3 of the persisted backend daily loop test-first: exact attempt scoring, published-content-day streak rules, immutable attempt/ledger/idempotency records, the single mutable `UserProgress` aggregate, EF ownership/constraint mappings, and a forward migration.
- Added PostgreSQL fresh-database and upgrade-from-current migration tests, but did not mark Spec 001 tasks or traceability evidence complete because local Docker remains unhealthy and hosted execution is still required.

### Files Changed

- `src/backend/Majlis.Domain/Progress/` - exact scoring, `UserProgress`, `UserAttempt`, `XpLedgerEntry`, and `IdempotencyRecord`.
- `src/backend/Majlis.Infrastructure/Persistence/` - DbSets, EF configurations, composite ownership keys, immutable after-save behavior, named checks/indexes/foreign keys, model snapshot, and migration `20260828114928_AddDailyLoopPersistence`.
- `src/backend/Majlis.Tests/` - scoring/streak boundary tests, persistence-model tests, and PostgreSQL fresh/upgrade migration tests.
- `MANIFEST.md` - registered all new Task 3 source, configuration, migration, and test files.
- `docs/ai-context/HANDOFF.md` - this task handoff.

### Decisions Made

- Kept `UserProgress` as the sole authority for lifetime XP, current streak, longest streak, last completed publish date, and its update timestamp; no `UserStreak` authority was added.
- Calculated streak continuity from intervening published `PublishDate` values: calendar gaps without publication are exempt, an intervening publication resets the next completion, and a repeated completion date is a complete no-op.
- Derived and constrained attempt XP so completion is always 10 and correctness is 5 only for a correct result; immutable attempts store the accepted locale and exact post-award XP/streak snapshots.
- Used restrictive content ownership foreign keys so accepted history cannot be removed through content deletion, while user-owned attempt/progress/idempotency data remains cascade-purgeable for account deletion.

### Tests and Checks Run

- RED: `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~XpAwardTests|FullyQualifiedName~UserProgressServiceTests|FullyQualifiedName~DailyLoopPersistenceModelTests"` failed with CS0234/CS0246 because the Progress domain types did not exist.
- GREEN: the same focused scoring/streak/model filter passed 12 tests, 0 failed.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-restore --filter "FullyQualifiedName!~Majlis.Tests.Integration"` - passed: 46 tests, 0 failed, 0 skipped.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed with 0 warnings and 0 errors.
- Repository-pinned `dotnet-ef` 10.0.11 generated `20260828114928_AddDailyLoopPersistence`; `migrations has-pending-model-changes` passed and the idempotent script contained the new tables, exact checks, ownership constraints, and ordered history index.
- Scoped `dotnet format --verify-no-changes` for all Task 3 C# files passed. Repository-wide format verification remains blocked by the existing CRLF/LF baseline in unrelated files.
- `git diff --check` passed; only expected line-ending conversion warnings were printed.

### Known Blockers

- `DailyLoopMigrationTests` fresh-database and upgrade-from-current cases were authored and compile, but were not run locally: localhost PostgreSQL refused connections during EF migration scaffold cleanup and repeated `docker info` checks did not return a healthy engine response. The controller must run the hosted PostgreSQL CI before relying on runtime migration evidence.
- Transactional submission, API/idempotency replay, authorization, concurrency, history, result retrieval, and rate limiting remain Task 4/5 work; no Spec 001 task or traceability status was changed in this checkpoint.

### Next Recommended Task

Run the Task 3 commit through hosted Backend CI, including the new fresh/upgrade migration tests. If green, implement Task 4 test-first: the transactional attempt/ledger/progress/idempotency application flow and owned result/history/progress APIs.

### 2026-08-28 - Persisted Daily-Loop Contract and Traceability Clarification

- Clarified the planned backend-only daily-loop contract before feature code; Flutter, admin UI, share rendering, and deep-link behavior remain out of scope for this task.

### Files Changed

- `specs/001-playable-daily-majlis/spec.md` and `tasks.md` - current-published-only submission, immutable result behavior, stored snapshots, and `UserProgress` wording.
- `docs/architecture/API_CONTRACTS.md` and `DATABASE_SCHEMA.md` - stored result locale/snapshot fields, non-enumerating reads, stable cursor behavior, and immutable attempt semantics.
- `docs/quality/requirements-to-tests.md` - more precise planned ATT/PROG evidence; no evidence status changed.
- `docs/ai-context/HANDOFF.md` - this task handoff.

### Decisions Made

- `UserProgress` remains the single authority for lifetime XP, current streak, longest streak, and last completed publish date; the stale `UserStreak` checklist item was reconciled rather than duplicated.
- New attempts are limited to the current UTC-date, currently `published` Daily Majlis; already accepted owned results remain immutable and retrievable after correction or unpublishing.
- Result retrieval replays the accepted result's stored BCP 47 locale, immutable source revision, and exact post-award lifetime-XP/current-streak/longest-streak snapshots.
- Attempt history uses newest-first opaque keyset cursors with `(AttemptedAt, Id)` as the stable boundary; missing and non-owned attempt reads share `404 attempt_not_found`.

### Tests and Checks Run

- Documentation validation and diff checks are required for this documentation-only task; no executable feature tests were added or claimed.

### Known Blockers

- The persisted attempts/XP/streak implementation and its executable evidence remain planned; all affected traceability rows remain `Planned`.

### Next Recommended Task

Implement Task 3 test-first: domain rules, `UserAttempt`, `XpLedgerEntry`, `UserProgress`, `IdempotencyRecord`, and an explicit forward migration.

### 2026-08-28 - Backend CI and Identity PostgreSQL Baseline

- Added GitHub Actions backend CI for pull requests and pushes to `main` and `feat/**`.
- The Ubuntu runner restores and builds the Release solution, verifies its hosted Linux Docker engine, runs the three `IdentityProfilePostgreSqlTests` first, then runs the complete backend Release suite.
- CI writes focused and full-suite TRX results and uploads them only when a workflow step fails; repository permissions are read-only.

### Files Changed

- `.github/workflows/backend-ci.yml` - backend CI workflow.
- `docs/superpowers/plans/2026-08-28-persist-backend-daily-loop.md` - implementation plan recorded for the persisted daily-loop branch.
- `docs/ai-context/HANDOFF.md` - this task handoff.

### Decisions Made

- Relied on GitHub-hosted Ubuntu’s default Docker endpoint for Testcontainers and verified it with `docker info` before PostgreSQL integration execution.
- Kept test-result artifacts failure-only and emitted distinct named TRX files under `TestResults`.

### Tests and Checks Run

- Workflow structural/content validation passed, including triggers, permissions, action versions, .NET version, focused-before-full execution order, TRX output paths, and failure-only artifact condition.
- `npx --yes prettier@3.7.4 --check .github/workflows/backend-ci.yml` - passed.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1` - passed: 64 Markdown files and 147 requirement ids.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed: 0 warnings and 0 errors.
- `git diff --check` - passed.
- `docker info` - failed against the local Docker Desktop Linux engine with HTTP 500; no local PostgreSQL test run was attempted.
- GitHub-hosted execution of the PostgreSQL identity baseline remains required before Task 2 because local Docker Desktop is unhealthy.

### Known Blockers

- Local Docker Desktop remains unhealthy, so the PostgreSQL baseline cannot be reproven locally; the new GitHub Actions workflow must pass before Task 2 starts.

### Next Recommended Task

Wait for the `Backend CI` workflow to pass its identity PostgreSQL baseline, then begin Task 2 contract and traceability clarification.

### 2026-08-28 - Daily Majlis Pre-Merge Review Corrections

- Recovered the Docker Desktop Linux engine and reran the previously blocked PostgreSQL suite.
- Fixed clean Development/Testing initialization so the new Daily Majlis aggregate and revision are saved before the publication pointer is assigned, with both saves committed in one transaction.
- Preserved prior published days during UTC rollover, gave scheduled or published editorial content precedence over seed content, and made concurrent startup converge on one official row.
- Added an upgrade repair for the immediately preceding checkpoint's known fixed-id seed so a published-but-mutable revision is atomically replaced with a complete submitted revision; concurrent legacy-seed repairs converge on the winner.
- Enforced domain publication through complete submitted revisions, rejected mutable or incomplete revisions in the serving path, and loaded persisted region provenance for API responses.
- Enforced server-owned terms and privacy versions before any profile bootstrap persistence.
- Added a forward-only migration boundary so a downgrade cannot reach the historical destructive localized-content rollback; recovery is by compatible backup restore or reviewed forward migration.
- Updated integration-test setup for the localized revision ownership model so unavailable-content and duplicate-publish-date tests exercise their intended API/database behavior.
- This repair makes the current checkpoint a reviewed merge candidate; the broader localized-revision slice remains incomplete and no specification task checkbox was closed.

### Files Changed

- Daily content domain/application/persistence: `DailyMajlis.cs`, `DailyMajlisService.cs`, `EfDailyMajlisRepository.cs`, and `DailyMajlisDatabaseInitializer.cs`.
- Identity consent enforcement: `RequiredConsentVersions.cs`, `IdentityProfileService.cs`, API composition/configuration, and their unit/PostgreSQL tests.
- Migration safety: `20260828064802_EstablishForwardOnlyLocalizedContentBoundary` plus its designer and the database-schema recovery guidance.
- Daily Majlis domain/application/PostgreSQL tests, the PostgreSQL 17 Testcontainers fixture, this handoff, the pre-merge implementation plan, and `MANIFEST.md`.

### Decisions Made

- Kept `PublishedRevisionId` nullable during the first persistence phase, then assigned it before committing the same database transaction, avoiding the EF insert cycle without weakening atomic publication.
- Treated any current-day scheduled or published row as authoritative and never moved an earlier seed aggregate to a later date.
- Reserved the historical fixed seed id for upgrade repair: an already-complete submitted seed remains idempotent, while mutable/incomplete legacy seed state is repaired before it can be accepted as official.
- Required publication revisions to belong to their aggregate, be submitted/immutable, and contain complete Arabic serving content.
- Stored required consent versions in server configuration and compared them exactly before repository lookup or user construction.
- Preserved applied migration history by adding an explicit forward-only boundary rather than editing the earlier localized-content migration.
- Represented unavailable content in the API integration test by unpublishing the current aggregate instead of deleting revision-owned content.
- Kept the duplicate-date test focused on the partial unique index by omitting the unrelated publication pointer from its candidate insert.
- Kept PostgreSQL at version 17 while moving the disposable integration fixture from the Alpine image to the official Debian image after repeated Alpine `initdb` stalls on Docker Desktop.

### Tests and Checks Run

- Red phase: the recovered PostgreSQL suite failed 11 integration tests on the EF `DailyMajlis`/`DailyMajlisRevision` publication-pointer cycle.
- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~DailyMajlisApiTests.Initializer_WhenRunMoreThanOnce_RemainsIdempotent"` - passed: 1 test, 0 failed after the transactional fix.
- A subsequent full run exposed two stale integration-test setup assumptions; their focused rerun passed: 2 tests, 0 failed.
- Red phase: publication/rollover tests did not compile before the explicit schedule/publish API existed; focused domain and PostgreSQL verification then passed: 11 tests, 0 failed.
- Red phase: fabricated consent versions were accepted and the PostgreSQL API returned Created; focused identity verification then passed: 8 tests, 0 failed.
- Red phase: the forward-only boundary test failed because the named migration did not exist; its focused PostgreSQL rerun passed: 1 test, 0 failed.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-restore` - passed: 61 tests, 0 failed, 0 skipped against PostgreSQL 17.
- Before the final legacy-seed correction, an isolated rerun passed 16 integration tests and 45 non-integration tests, for the then-complete 61-test coverage with 0 failures and 0 skipped.
- Reviewer regression red phase: the mutable published seed remained mutable and concurrent unpublished-seed repair raised PostgreSQL unique violation `23505`.
- Focused reviewer regression rerun - passed: 2 tests, 0 failed, covering mutable published-seed upgrade and concurrent unpublished-seed repair.
- Current Daily Majlis PostgreSQL class rerun - passed: 15 tests, 0 failed, 0 skipped.
- Current non-integration rerun - passed: 45 tests, 0 failed, 0 skipped.
- Current combined 63-test run was aborted by an `Internal CLR error` in the .NET/Npgsql query host after all 45 non-integration tests passed; the three identity PostgreSQL tests were subsequently blocked before execution by Docker Desktop/Testcontainers readiness stalls. Do not treat the combined run as passed.
- Final focused re-review of `b9a7394..d4a1de1` reported no Critical, Important, or Minor findings and returned `Ready to merge`.
- `dotnet format src/backend/Majlis.sln --verify-no-changes --no-restore` - passed.
- `dotnet tool restore` - restored repository-pinned `dotnet-ef` 10.0.11.
- `dotnet tool run dotnet-ef migrations has-pending-model-changes ... --configuration Release --no-build` - passed with no pending model changes.
- `dotnet tool run dotnet-ef migrations script --idempotent ... --configuration Release --no-build` - passed.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1` - passed: 61 Markdown files and 147 requirement ids.
- `git diff --check` - passed; only expected line-ending conversion warnings were printed.

### Known Blockers

- The localized revision slice still needs its remaining locale edge cases and content-management/import workflow before its specification task can close.
- Flutter, attempts, XP, streaks, and the rest of the complete Production V1 remain outside this checkpoint and are still required for `Game Ready`.
- Production provider credentials, hosting, domains, verified links, and signing remain intentionally deferred until `Game Ready`.
- The local Docker Desktop engine currently becomes unhealthy across repeated Testcontainers lifecycles; the three unchanged identity PostgreSQL cases need a fresh CI or stable-engine rerun even though they passed before the final initializer-only correction.

### Next Recommended Task

Complete the remaining locale/content-management cases test-first, then implement persisted attempts, scoring, XP, and streaks as the next end-to-end Daily Majlis slice.

### 2026-08-27 - Localized Daily Majlis Revision Checkpoint

- Added the in-progress localized content-revision domain, persistence mappings, forward migrations, locale negotiation, canonical Today response fields, Arabic/English Development seed content, and focused tests.
- This is a remote checkpoint for continued work on Spec 001. The localized revision slice is not complete and no task checkbox was closed.

### Files Changed

- Daily content domain and contracts under `src/backend/Majlis.Domain/DailyMajlis/` and `src/backend/Majlis.Contracts/DailyMajlis/`.
- Today query/controller localization behavior under `src/backend/Majlis.Application/DailyMajlis/` and `src/backend/Majlis.Api/Controllers/`.
- EF mappings, initializer, repository, model snapshot, and forward migrations under `src/backend/Majlis.Infrastructure/`.
- Focused domain, application, and PostgreSQL integration tests under `src/backend/Majlis.Tests/`.

### Decisions Made

- Kept PostgreSQL as the authoritative store and retained the global UTC content day.
- Localized consumer content is served from immutable revision-owned translations, with complete Arabic required and BCP 47 fallback behavior.
- Production identity providers, hosting, domains, verified links, and signing remain deferred until `Game Ready`.

### Tests and Checks Run

- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "Category!=Integration"` - passed: 41 tests, 0 failed, 0 skipped.
- `dotnet format src/backend/Majlis.sln --verify-no-changes --no-restore` - passed.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1` - passed: 60 Markdown files and 147 requirement ids.
- `git diff --check` - passed; only expected line-ending conversion warnings were printed.
- `docker info` - failed because the Docker Desktop Linux engine pipe was unavailable, so PostgreSQL-backed integration verification was not rerun.
- `dotnet tool run dotnet-ef migrations has-pending-model-changes ...` could not run because the local tool was not restored; the subsequent tool restore stalled against the configured package source and was stopped.

### Known Blockers

- PostgreSQL-backed clean initialization, forward-migration, rollover, scheduled-content conflict, and concurrency cases remain unverified while Docker is unavailable.
- The initializer still requires follow-up for scheduled-content precedence, repeated day rollover, transactional publication-pointer assignment, and submitted-revision immutability before this slice can be marked complete.
- Migration handling for legacy published rows and nullable Development/import provenance requires final review against the target schema.

### Next Recommended Task

Recover Docker, add the missing initializer and locale edge-case tests, fix the resulting failures test-first, then rerun the full PostgreSQL suite and migration checks before completing the localized-revision slice.

## Latest Task Completed

### 2026-08-26 - Local Identity and Profile Foundation

- Expanded the Production V1 provider set to Google Account, Sign in with Apple, Meta/Facebook Login, and Snapchat Login Kit behind one external-identity boundary.
- Added the `UserAccount` aggregate with provider identities, profile, default preferences, versioned consents, role assignments, session revocation, and deletion-request state.
- Added explicit provider/issuer/subject and per-user/provider uniqueness, private leaderboard defaults, 13+ age-band validation, normalized display/profile fields, and deletion deadlines.
- Added a Development/Testing-only signed JWT issuer; Production fails closed if test authentication is configured.
- Added authenticated profile bootstrap, read/update, revoke-all, and deletion-request endpoints with safe problem codes.
- Protected the Daily Majlis endpoint with the completed-profile policy.
- Added migration `AddIdentityProfileFoundation` without altering the existing migration.
- Added domain, application, token/configuration, persistence-model, functional API, and PostgreSQL integration tests.
- Reordered delivery so the local persisted Arabic daily loop reaches `Game Ready` before external credentials, hosting, domains, App Links, signing, staging, or deployment work begins.

### Files Changed

- Identity domain/contracts/application: `src/backend/Majlis.Domain/Identity/`, `src/backend/Majlis.Contracts/Identity/`, and `src/backend/Majlis.Application/Identity/`.
- API/authentication: `src/backend/Majlis.Api/Authentication/`, identity/profile controllers, `Program.cs`, app settings, and HTTP samples.
- Persistence: identity repository, EF configurations, DbContext registration, enum storage, and the new migration/snapshot.
- Tests: identity domain/application/configuration/model/functional API/PostgreSQL suites plus authenticated Daily Majlis integration setup.
- Specifications and guidance: identity/provider decisions, delivery sequencing, API/schema contracts, traceability, manifest, README, AGENTS, and this handoff.

### Decisions Made

- Production providers are Google, Apple, Meta, and Snapchat; other login methods remain out of scope.
- Provider mechanics remain isolated behind the external-identity boundary because Meta/Snapchat are not assumed to share Google/Apple OIDC behavior.
- A local user may link at most one identity per supported provider; email equality never links accounts.
- Local feature work uses an ephemeral signed test issuer that cannot start in Production.
- External credentials/callbacks and hosting/domain/signing/staging work begin only after the documented `Game Ready` gate.

### Tests and Checks Run

- Red phase: the new tests initially failed on missing identity domain types and persistence sets.
- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "Category!=Integration"` - passed: 33 tests, 0 failed, 0 skipped, including domain, application, signed-token, malformed-token, fail-closed configuration, persistence-model, and functional API coverage.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed with 0 warnings and 0 errors.
- `dotnet format src/backend/Majlis.sln --verify-no-changes --no-restore` - passed.
- `dotnet tool run dotnet-ef migrations has-pending-model-changes ... --configuration Release` - passed with no pending model changes.
- `dotnet tool run dotnet-ef migrations script --idempotent ... --configuration Release` - passed after a transient concurrent EF build-host collision was rerun sequentially.
- `dotnet list src/backend/Majlis.sln package --vulnerable --include-transitive` - completed after adding JwtBearer with no known vulnerable packages; a later refresh attempt was blocked by the configured NuGet proxy with HTTP 407 and no package change had occurred.
- Documentation validation passed for 60 Markdown files and 147 requirement ids.

### Known Blockers

- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --no-restore` compiled successfully but the PostgreSQL-backed cases could not start because Docker Desktop's Linux engine returned HTTP 500 on both contexts. One Docker Desktop restart did not recover it; a Windows reboot is the next appropriate local recovery step.
- Live Google/Apple/Meta/Snapchat adapters, provider linking endpoints, and provider revocation remain intentionally deferred until after `Game Ready`.
- Role-management endpoints, auth/profile rate limiting, deletion purge/retention jobs, and the Flutter identity UI remain incomplete.

### Next Recommended Task

After rebooting Windows, rerun the full PostgreSQL-backed suite. If green, implement the persisted one-attempt, XP-ledger, and UTC streak slice from Spec 001 using the completed local identity/profile foundation.

## Previous Work

### 2026-08-26 - Production V1 Specification Hardening

- Reconciled family scope, UTC publishing, regional semantics, Arabic launch behavior, comments, gameplay, privacy, and retention in one normative V1 decision register.
- Added focused spec/plan/task triplets for authentication/profile, leaderboard, sharing/deep links, reminders, content/moderation administration, and production operations.
- Defined exact scoring, retries, streak boundaries, idempotency/concurrency, comment visibility, blocking, appeals, age safeguards, and account deletion behavior.
- Replaced the draft API and database documents with complete V1 target contracts, including localized immutable content revisions and required source notes.
- Added measurable performance, Android-version, accessibility, reliability, security, analytics, backup/restore, deployment, and release gates.
- Added requirement-to-test traceability and a repository-local validation hook.
- Archived the stale foundation work plan and made the reusable feature prompt select its focused spec instead of hardcoding Spec 001.
- Fixed Google, Apple, Meta/Facebook Login, and Snapchat Login Kit as the V1 identity choices and deferred production identity/hosting/domain logistics until after the local `Game Ready` milestone.

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
- Google, Apple, Meta, and Snapchat own account authentication/recovery; Majlis owns explicit identity linking, local users, roles, privacy, and deletion state and never merges by email.
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
- Google/Apple/Meta/Snapchat production credentials and verification, provider callback configuration, hosting, canonical public hosts, signing fingerprints, staging, and deployment are deliberately deferred until `Game Ready`; they remain release gates.
- Legal/product review must confirm enabled launch jurisdictions for 13-17 accounts and the documented retention windows before production launch.

### Next Recommended Task

Implement the provider-neutral portion of Spec 004 test-first: add signed-test-token infrastructure, local user/identity/profile/consent/deletion persistence, and cross-user authorization tests without production Google/Apple credentials or hosting dependencies.

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

At this point in history, authentication/profile specification and provider choice were still open. The later hardening decision selects Google, Apple, Meta, and Snapchat and moves their production configuration after `Game Ready`.

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

- After `Game Ready`, configure Google, Apple, Meta, and Snapchat production credentials/callbacks and verify all four in staging.
- After `Game Ready`, select the hosting/managed PostgreSQL providers and record the Spec 009 RPO/RTO and data-residency evidence.
- After `Game Ready`, configure the canonical production/staging hosts and Android signing fingerprints for verified App Links.
- Confirm launch jurisdictions and obtain legal/product approval for minor accounts and retention.
- Name the product, editorial, security/privacy, engineering, and operations release approvers.

## Last Updated

2026-08-28
