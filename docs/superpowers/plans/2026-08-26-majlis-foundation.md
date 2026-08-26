# Majlis Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first playable Majlis product slice: Android Flutter app plus .NET backend daily cultural challenge loop.

**Architecture:** Flutter renders the daily Majlis experience and submits answers. .NET owns content, validation, scoring, streaks, and moderation state. PostgreSQL persists users, content, attempts, streaks, and discussion records.

**Tech Stack:** Flutter, Riverpod, GoRouter, .NET 10 LTS, ASP.NET Core, EF Core, PostgreSQL, xUnit.

**Spec:** `specs/001-playable-daily-majlis/spec.md`

## Global Constraints

- Android MVP first.
- Daily challenge must be completable in 1-3 minutes.
- Backend must not expose correct answer before answer submission.
- No public community feature ships without reporting/moderation status.
- Content must not be hardcoded into Flutter screens.
- Copy must be playful and non-shaming.

---

### Task 1: Backend Solution Skeleton

**Files:**
- Create: `src/backend/Majlis.sln`
- Create: `src/backend/Majlis.Api/`
- Create: `src/backend/Majlis.Application/`
- Create: `src/backend/Majlis.Domain/`
- Create: `src/backend/Majlis.Infrastructure/`
- Create: `src/backend/Majlis.Contracts/`
- Create: `src/backend/Majlis.Tests/`

**Interfaces:**
- Produces: .NET clean architecture solution used by later backend tasks.

- [ ] **Step 1: Create solution and projects**

```bash
cd src/backend
dotnet new sln -n Majlis
dotnet new webapi -n Majlis.Api
dotnet new classlib -n Majlis.Application
dotnet new classlib -n Majlis.Domain
dotnet new classlib -n Majlis.Infrastructure
dotnet new classlib -n Majlis.Contracts
dotnet new xunit -n Majlis.Tests
```

- [ ] **Step 2: Add projects to solution**

```bash
dotnet sln add Majlis.Api/Majlis.Api.csproj
dotnet sln add Majlis.Application/Majlis.Application.csproj
dotnet sln add Majlis.Domain/Majlis.Domain.csproj
dotnet sln add Majlis.Infrastructure/Majlis.Infrastructure.csproj
dotnet sln add Majlis.Contracts/Majlis.Contracts.csproj
dotnet sln add Majlis.Tests/Majlis.Tests.csproj
```

- [ ] **Step 3: Add references**

```bash
dotnet add Majlis.Application/Majlis.Application.csproj reference Majlis.Domain/Majlis.Domain.csproj Majlis.Contracts/Majlis.Contracts.csproj
dotnet add Majlis.Infrastructure/Majlis.Infrastructure.csproj reference Majlis.Application/Majlis.Application.csproj Majlis.Domain/Majlis.Domain.csproj
dotnet add Majlis.Api/Majlis.Api.csproj reference Majlis.Application/Majlis.Application.csproj Majlis.Infrastructure/Majlis.Infrastructure.csproj Majlis.Contracts/Majlis.Contracts.csproj
dotnet add Majlis.Tests/Majlis.Tests.csproj reference Majlis.Application/Majlis.Application.csproj Majlis.Domain/Majlis.Domain.csproj Majlis.Contracts/Majlis.Contracts.csproj
```

- [ ] **Step 4: Build**

```bash
dotnet build
```

Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add src/backend
git commit -m "feat: create backend solution skeleton"
```

### Task 2: Daily Majlis Domain Model

**Files:**
- Create: `src/backend/Majlis.Domain/DailyMajlis/DailyMajlis.cs`
- Create: `src/backend/Majlis.Domain/Challenges/Challenge.cs`
- Create: `src/backend/Majlis.Domain/Challenges/ChallengeOption.cs`
- Create: `src/backend/Majlis.Domain/Attempts/UserAttempt.cs`
- Create: `src/backend/Majlis.Domain/Streaks/UserStreak.cs`
- Test: `src/backend/Majlis.Tests/Domain/ChallengeTests.cs`

**Interfaces:**
- Produces: domain entities for challenge and streak logic.

- [ ] **Step 1: Write failing tests for challenge correctness rule**

Create `ChallengeTests.cs` with tests that assert a multiple-choice challenge must have exactly one correct option.

- [ ] **Step 2: Implement domain entities**

Create focused entities with IDs, text fields, correct-option rules, attempt uniqueness handled by application layer, and streak update methods.

- [ ] **Step 3: Run tests**

```bash
cd src/backend
dotnet test
```

Expected: Tests pass.

- [ ] **Step 4: Commit**

```bash
git add src/backend/Majlis.Domain src/backend/Majlis.Tests
git commit -m "feat: add daily majlis domain model"
```

### Task 3: Flutter App Skeleton

**Files:**
- Create: `apps/mobile/`
- Create: `apps/mobile/lib/core/`
- Create: `apps/mobile/lib/features/daily_majlis/`
- Create: `apps/mobile/lib/features/challenge/`
- Create: `apps/mobile/lib/features/results/`

**Interfaces:**
- Produces: Flutter app foundation used by UI tasks.

- [ ] **Step 1: Create Flutter app**

```bash
cd apps
flutter create mobile
```

- [ ] **Step 2: Add packages**

```bash
cd mobile
flutter pub add flutter_riverpod go_router dio
```

- [ ] **Step 3: Create feature-first folders**

```bash
mkdir -p lib/core/api lib/core/routing lib/core/theme lib/core/widgets
mkdir -p lib/features/onboarding lib/features/daily_majlis lib/features/challenge lib/features/results lib/features/discussion lib/features/profile lib/features/leaderboard
```

- [ ] **Step 4: Analyze**

```bash
flutter analyze
```

Expected: No errors.

- [ ] **Step 5: Commit**

```bash
git add apps/mobile
git commit -m "feat: create flutter app skeleton"
```

### Task 4: Implement Playable Daily Majlis API and UI

**Files:**
- Modify: backend Application/API/Infrastructure files from Tasks 1-2
- Modify: `apps/mobile/lib/features/daily_majlis/`
- Modify: `apps/mobile/lib/features/challenge/`
- Modify: `apps/mobile/lib/features/results/`

**Interfaces:**
- Consumes: domain model and Flutter skeleton.
- Produces: first playable daily loop.

- [ ] **Step 1: Implement backend contracts**

Use `docs/architecture/API_CONTRACTS.md` for DTOs.

- [ ] **Step 2: Implement today's Majlis endpoint**

Endpoint: `GET /api/v1/daily-majlis/today`.

- [ ] **Step 3: Implement answer submission endpoint**

Endpoint: `POST /api/v1/challenges/{challengeId}/attempts`.

- [ ] **Step 4: Implement Flutter API client and screens**

Build today's screen, challenge selection, result screen, and share summary display.

- [ ] **Step 5: Validate**

```bash
cd src/backend && dotnet test
cd ../../apps/mobile && flutter analyze && flutter test
```

- [ ] **Step 6: Commit**

```bash
git add src/backend apps/mobile docs/ai-context/HANDOFF.md
git commit -m "feat: implement playable daily majlis loop"
```
