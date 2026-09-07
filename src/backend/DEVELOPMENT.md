# Backend Development Guide

This guide explains how to work on the Majlis backend safely and consistently.

## Required Reading

Before coding, read:

1. `AGENTS.md`
2. `docs/ai-context/PROJECT.md`
3. `docs/ai-context/ARCHITECTURE.md`
4. `docs/ai-context/CONVENTIONS.md`
5. `docs/ai-context/HANDOFF.md`
6. `.specify/memory/constitution.md`
7. Relevant `specs/<feature>/spec.md`, `plan.md`, and `tasks.md`

## Branch Naming

Use focused branch names:

```text
feat/daily-majlis-attempts
feat/streak-service
feat/flutter-daily-screen
fix/spoiler-safe-response
docs/backend-api-guide
```

## Commit Style

Use conventional commits:

```text
feat: add challenge attempt endpoint
fix: prevent correct answer leak in today response
test: cover duplicate attempt rejection
docs: update backend api guide
refactor: move scoring rule into domain service
```

## Local Commands

From repository root:

```powershell
cd src/backend
dotnet restore Majlis.sln
dotnet build Majlis.sln
dotnet test Majlis.sln
```

Format check:

```powershell
cd src/backend
dotnet format Majlis.sln --verify-no-changes --no-restore
```

Run API:

```powershell
cd src/backend/Majlis.Api
dotnet run
```

## Development Flow

1. Pull latest `main`.
2. Create a small feature branch.
3. Read the relevant spec and handoff.
4. Write or update tests first for business rules.
5. Implement the smallest working slice.
6. Run build/tests/format.
7. Update documentation if contracts or behavior changed.
8. Update `docs/ai-context/HANDOFF.md`.
9. Open a pull request.

## Coding Rules

- Keep controllers/endpoints thin.
- Put game and scoring rules in Application/Domain.
- Keep contracts explicit and mobile-safe.
- Do not expose correct answers before submission.
- Do not hardcode long-term cultural content inside API endpoints.
- Prefer dependency injection over static access.
- Prefer deterministic date/clock abstractions for tests.

## Feature Slice Definition

A backend feature slice is complete only when it has:

- API contract or domain model as needed
- Application behavior
- Infrastructure implementation or in-memory adapter
- API endpoint if externally visible
- Tests for main path and at least one failure path
- Handoff update

## Review Checklist

Before PR:

- [ ] Does the change follow clean architecture boundaries?
- [ ] Does the API avoid spoilers?
- [ ] Are tests included for game rules?
- [ ] Are errors predictable and typed?
- [ ] Is cultural copy respectful and non-insulting?
- [ ] Did you update `HANDOFF.md`?

## Handoff Template

```markdown
## YYYY-MM-DD - Task Title

### Completed
- 

### Files Changed
- 

### Decisions
- 

### Validation
- Command: `dotnet test src/backend/Majlis.sln`
- Result: 

### Blockers
- 

### Next Recommended Step
- 
```
