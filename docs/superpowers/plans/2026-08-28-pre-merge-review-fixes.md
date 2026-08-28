# Pre-Merge Review Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Resolve the merge-blocking data-integrity, publication, consent, rollback, and provenance findings on `feat/persist-daily-majlis` without claiming the localized-content slice or Production V1 is complete.

**Architecture:** Keep publication rules in the Daily Majlis domain, keep Development/Testing seed orchestration in Infrastructure, and validate server-owned consent versions in Application through an injected value object registered by the API composition root. Preserve applied migrations by adding a new forward-only migration boundary instead of rewriting historical migration code.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core 10, Npgsql/PostgreSQL 17, xUnit, Testcontainers.

**Spec:** `specs/001-playable-daily-majlis/spec.md`, `specs/003-production-app/spec.md`, `specs/004-authentication-profile/spec.md`

## Global Constraints

- `PublishDate` is the canonical UTC calendar date and existing published days must never be moved forward.
- Scheduled or published editorial content takes precedence over Development/Testing seed content.
- A served revision must be complete in Arabic and immutable.
- Terms and privacy versions are server-owned exact values; a rejected bootstrap persists no user or consent.
- Do not edit an applied migration; establish rollback safety through a new migration boundary.
- Keep the checkpoint and handoff honest: these fixes do not complete Spec 001, `Game Ready`, or Production V1.

---

### Task 1: Preserve Daily Content History and Seal Publications

**Files:**
- Modify: `src/backend/Majlis.Domain/DailyMajlis/DailyMajlis.cs`
- Modify: `src/backend/Majlis.Infrastructure/Persistence/DailyMajlisDatabaseInitializer.cs`
- Modify: `src/backend/Majlis.Application/DailyMajlis/DailyMajlisService.cs`
- Test: `src/backend/Majlis.Tests/Domain/DailyMajlisRevisionTests.cs`
- Test: `src/backend/Majlis.Tests/Integration/DailyMajlisApiTests.cs`

**Interfaces:**
- Produces: `DailyMajlis(Guid id, DateOnly publishDate)`, `Schedule(DailyMajlisRevision)`, and `Publish(DailyMajlisRevision)`.
- Consumes: `DailyMajlisRevision.Submit(DateTimeOffset)`, `IsImmutable`, and `IsCompleteForServing()`.

- [x] **Step 1: Add failing domain and PostgreSQL tests**

```csharp
[Fact]
public void Publish_WhenRevisionIsMutable_RejectsPublication()
{
    var revision = CreateCompleteRevision();
    var daily = new DailyMajlis(Guid.NewGuid(), new DateOnly(2026, 8, 26));
    Assert.Throws<InvalidOperationException>(() => daily.Publish(revision));
}

[Fact]
public async Task Initializer_WhenUtcDayAdvances_PreservesPriorPublishedDay()
{
    // Initialize 2026-08-26, initialize again with a 2026-08-27 TimeProvider,
    // then assert two distinct published aggregates and both dates remain stored.
}
```

- [x] **Step 2: Run focused tests and verify failures name missing publication operations, mutable serving, scheduled precedence, rollover, or concurrent startup**

Run: `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~DailyMajlisRevisionTests|FullyQualifiedName~DailyMajlisApiTests.Initializer"`

- [x] **Step 3: Implement explicit schedule/publish rules and history-safe seeding**

```csharp
public void Publish(DailyMajlisRevision revision)
{
    ValidatePublicationRevision(revision);
    Status = DailyMajlisStatus.Published;
    ScheduledRevision = null;
    ScheduledRevisionId = null;
    PublishedRevision = revision;
    PublishedRevisionId = revision.Id;
}

private void ValidatePublicationRevision(DailyMajlisRevision revision)
{
    if (revision.DailyMajlisId != Id || !revision.IsImmutable || !revision.IsCompleteForServing())
        throw new InvalidOperationException("Only a complete submitted revision belonging to this Daily Majlis may be published.");
}
```

The initializer must return when any current-day row is `scheduled` or `published`, seal Development/Testing revisions with `Submit`, use fresh aggregate/challenge ids for each new UTC day, preserve legacy seed rows from earlier days, and catch only PostgreSQL unique violations that converge on a concurrently inserted official row.

- [x] **Step 4: Require `revision.IsImmutable` in the serving path and include persisted regions in the repository query**

```csharp
if (dailyMajlis is null || revision is null || !revision.IsImmutable || !revision.IsCompleteForServing())
    return null;
```

- [x] **Step 5: Run the focused domain and PostgreSQL tests until green**

Run the Step 2 command and the Today API tests; expected result is zero failures.

### Task 2: Enforce Server-Owned Consent Versions

**Files:**
- Create: `src/backend/Majlis.Application/Identity/RequiredConsentVersions.cs`
- Modify: `src/backend/Majlis.Application/Identity/IdentityProfileService.cs`
- Modify: `src/backend/Majlis.Api/Program.cs`
- Modify: `src/backend/Majlis.Api/appsettings.json`
- Modify: `src/backend/Majlis.Api/appsettings.Development.json`
- Test: `src/backend/Majlis.Tests/Application/IdentityProfileServiceTests.cs`
- Test: `src/backend/Majlis.Tests/Integration/IdentityProfilePostgreSqlTests.cs`

**Interfaces:**
- Produces: `RequiredConsentVersions(string Terms, string Privacy)` injected into `IdentityProfileService`.
- Consumes: `BootstrapProfileRequest.AcceptedTermsVersion` and `AcceptedPrivacyVersion`.

- [x] **Step 1: Add failing unit and PostgreSQL tests for fabricated terms/privacy versions**

```csharp
var exception = await Assert.ThrowsAsync<IdentityProfileException>(() =>
    service.BootstrapAsync(identity, request with { AcceptedTermsVersion = "fabricated" }, CancellationToken.None));
Assert.Equal("validation_failed", exception.Code);
Assert.Empty(repository.Users);
Assert.Equal(0, repository.SaveCount);
```

The API/PostgreSQL test must assert `422` and zero persisted users/consents.

- [x] **Step 2: Run the new tests and verify they fail because arbitrary versions are currently accepted**

Run: `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~IdentityProfileServiceTests|FullyQualifiedName~IdentityProfilePostgreSqlTests"`

- [x] **Step 3: Add exact server-owned validation before repository lookup or user construction**

```csharp
if (!string.Equals(request.AcceptedTermsVersion, requiredConsentVersions.Terms, StringComparison.Ordinal) ||
    !string.Equals(request.AcceptedPrivacyVersion, requiredConsentVersions.Privacy, StringComparison.Ordinal))
    throw new IdentityProfileException("validation_failed", "Accept the current terms and privacy versions.");
```

Register non-empty `ConsentVersions:Terms` and `ConsentVersions:Privacy` values in `Program.cs`; invalid or missing server configuration must fail startup.

- [x] **Step 4: Rerun the focused unit and PostgreSQL tests until green**

Expected: valid current versions persist exactly two required consents; mismatches persist nothing.

### Task 3: Establish a Forward-Only Localized-Migration Boundary

**Files:**
- Create: generated migration `src/backend/Majlis.Infrastructure/Persistence/Migrations/*_EstablishForwardOnlyLocalizedContentBoundary.cs`
- Create: its generated designer file
- Modify: `src/backend/Majlis.Infrastructure/Persistence/Migrations/MajlisDbContextModelSnapshot.cs` only if generated tooling changes metadata
- Modify: `docs/architecture/DATABASE_SCHEMA.md`
- Test: `src/backend/Majlis.Tests/Integration/DailyMajlisApiTests.cs`

**Interfaces:**
- Produces: an empty `Up` migration whose `Down` throws `NotSupportedException` before the older lossy localized-content downgrade can execute.

- [x] **Step 1: Add a failing integration test that requires the named boundary migration and verifies a downgrade request is rejected while localized tables remain intact**

```csharp
var migrations = (await dbContext.Database.GetMigrationsAsync()).ToArray();
var boundary = Assert.Single(migrations, id => id.EndsWith("_EstablishForwardOnlyLocalizedContentBoundary", StringComparison.Ordinal));
await dbContext.Database.MigrateAsync();
await Assert.ThrowsAsync<NotSupportedException>(() => dbContext.Database.MigrateAsync(migrations[^2]));
Assert.NotNull(await dbContext.DailyMajlisRevisions.FirstOrDefaultAsync());
```

- [x] **Step 2: Run the test and verify it fails because the boundary migration does not exist**

Run: `dotnet test src/backend/Majlis.Tests/Majlis.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~ForwardOnlyLocalizedContentBoundary"`

- [x] **Step 3: Generate and harden the boundary migration**

Run: `dotnet tool run dotnet-ef migrations add EstablishForwardOnlyLocalizedContentBoundary --project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --startup-project src/backend/Majlis.Api/Majlis.Api.csproj --configuration Release`

Keep `Up` empty and implement:

```csharp
protected override void Down(MigrationBuilder migrationBuilder) =>
    throw new NotSupportedException("Localized content revisions are a forward-only boundary; restore a compatible backup or apply a reviewed forward recovery migration.");
```

- [x] **Step 4: Document the forward-only boundary and rerun the focused migration test**

The schema document must state that rollback across this boundary uses backup restore or a reviewed forward recovery migration, never the historical destructive `Down` path.

### Task 4: Verify and Hand Off the Merge Candidate

**Files:**
- Modify: `docs/ai-context/HANDOFF.md`
- Modify: `docs/quality/requirements-to-tests.md` only when verified evidence status changes.

**Interfaces:**
- Consumes: all prior task outputs.
- Produces: one coherent conventional fix commit ready for review and merge.

- [x] **Step 1: Run complete verification**

```powershell
dotnet test src/backend/Majlis.sln --configuration Release --no-restore
dotnet format src/backend/Majlis.sln --verify-no-changes --no-restore
dotnet tool run dotnet-ef migrations has-pending-model-changes --project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --startup-project src/backend/Majlis.Api/Majlis.Api.csproj --configuration Release --no-build
dotnet tool run dotnet-ef migrations script --idempotent --project src/backend/Majlis.Infrastructure/Majlis.Infrastructure.csproj --startup-project src/backend/Majlis.Api/Majlis.Api.csproj --configuration Release --no-build
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-docs.ps1
git diff --check
```

- [x] **Step 2: Update the handoff with date, files, decisions, exact verification results, remaining blockers, and next recommended task**

- [ ] **Step 3: Commit the review corrections**

```powershell
git add -- src/backend docs
git commit -m "fix: address daily majlis merge review"
```

- [ ] **Step 4: Request a focused re-review of the correction range and resolve every Critical or Important finding before push**
