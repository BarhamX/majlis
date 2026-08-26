# AI Context: Handoff

## Current Status

The first Milestone 1 backend slice is complete: the .NET 10 clean-architecture solution builds, the Daily Majlis query returns seeded content, and `GET /api/v1/daily-majlis/today` returns a spoiler-safe payload.

## Task Completed

### 2026-08-26 - Initial Playable Daily Majlis Backend

- Created the backend solution and all six planned projects.
- Added `DailyMajlis`, `Challenge`, and `ChallengeOption` domain entities.
- Enforced exactly one correct option for a multiple-choice challenge.
- Added the API response DTOs and the `IDailyMajlisService` application contract.
- Implemented the date-aware Daily Majlis query and an in-memory seed repository.
- Added the versioned endpoint and replaced the template weather endpoint.
- Added application and domain unit tests.

## Files Changed

- `src/backend/Majlis.sln`
- `src/backend/Majlis.Api/` - API project, composition root, controller, host settings, and HTTP request sample.
- `src/backend/Majlis.Application/` - today's query service and repository/service abstractions.
- `src/backend/Majlis.Contracts/` - spoiler-safe Daily Majlis response contracts.
- `src/backend/Majlis.Domain/` - Daily Majlis aggregate, challenge, option, and supporting statuses.
- `src/backend/Majlis.Infrastructure/` - temporary in-memory seed repository.
- `src/backend/Majlis.Tests/` - application service and domain invariant tests.
- `specs/001-playable-daily-majlis/tasks.md`
- `docs/ai-context/HANDOFF.md`

## Decisions Made

- Used the canonical versioned route `/api/v1/daily-majlis/today` from the conventions and API contract.
- Kept the correct option, explanation, source notes, and editorial state in the Domain model; the pre-attempt response DTO exposes only option ids and text.
- Placed the temporary seeded repository in Infrastructure so it can be replaced without changing the controller or application contract.
- Used an injected `TimeProvider` and a UTC date boundary for deterministic tests. The product scheduling timezone remains to be decided before database-backed publishing.
- Returned a safe `404` problem response when no published Daily Majlis is available.
- Kept `userState` at `hasAttempted: false` and `currentStreak: 0` until authentication and attempt persistence are implemented.

## Tests and Checks Run

- `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj` - expected red phase failed with 5 missing-type compiler errors before implementation.
- `dotnet build src/backend/Majlis.sln --configuration Release --no-restore` - passed with 0 warnings and 0 errors.
- `dotnet test src/backend/Majlis.sln --configuration Release --no-build` - passed: 2 tests, 0 failed, 0 skipped.
- `dotnet run --project src/backend/Majlis.Api/Majlis.Api.csproj --configuration Release --no-build --no-launch-profile --urls https://127.0.0.1:5092` plus `curl.exe --insecure --fail-with-body --silent --show-error --write-out "`nHTTP %{http_code}`n" https://127.0.0.1:5092/api/v1/daily-majlis/today` - returned HTTP 200 with the expected JSON and no correct-answer field.

## Known Blockers

- None for this slice.
- PostgreSQL/EF Core, authentication, attempts, scoring, and persisted user state remain intentionally deferred.
- The final content scheduling timezone is not yet specified; the placeholder query currently uses UTC.

## Next Recommended Task

Add the health endpoint and PostgreSQL/EF Core configuration, create the first explicit migration for Daily Majlis content, replace the seed repository with persistence, and add an API integration test for the spoiler-safe response.

## Open Decisions

- Authentication provider: custom JWT vs managed identity provider.
- Hosting provider.
- Final Arabic font family.
- First regional content focus: Qatar/Gulf, pan-Arab, or mixed.
- Whether comments are visible immediately or pending review during beta.
- Daily content scheduling timezone.

## Last Updated

2026-08-26
