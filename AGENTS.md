# AGENTS.md

This file is the operational contract for AI coding agents working on Majlis.

## Project Summary

Majlis is a production Flutter Android mobile game with a .NET backend. It delivers daily Arab cultural challenges, proverbs, stories, discussion prompts, streaks, leaderboards, and shareable cultural cards as one complete, running application.

## Read First

Before making changes, read these files in order:

1. `docs/ai-context/PROJECT.md`
2. `docs/product/full-app-scope.md`
3. `docs/product/v1-product-decisions.md`
4. `docs/ai-context/ARCHITECTURE.md`
5. `docs/ai-context/CONVENTIONS.md`
6. `docs/ai-context/HANDOFF.md`
7. `.specify/memory/constitution.md`
8. `specs/003-production-app/spec.md`, `plan.md`, and `tasks.md`
9. `docs/architecture/API_CONTRACTS.md`, `DATABASE_SCHEMA.md`, and `docs/quality/requirements-to-tests.md`
10. The relevant feature `spec.md`, `plan.md`, and `tasks.md`

## Working Rules

- Treat specs as the source of truth.
- Treat the complete production Android app as the delivery target. Feature slices are sequencing tools, not reduced release boundaries.
- Do not describe Majlis as a prototype, demo, backend-only deliverable, or reduced release.
- Do not invent product behavior that is not in a spec.
- Requirement IDs must be mapped in `docs/quality/requirements-to-tests.md`; a task is not complete while mapped evidence remains planned.
- Keep changes small and reviewable.
- Prefer test-first development for backend domain logic and API behavior.
- Preserve cultural respect and authenticity in all copy and content behavior.
- Do not add addictive dark patterns. Retention must be based on ritual, learning, competition, and social belonging.
- Do not implement broad social-media behavior before moderation and reporting exist.
- Do not hardcode daily cultural content into Flutter screens. Daily content must come from backend or seed data.
- Use Development/Testing identity while building the local game. Do not provision production Google/Apple credentials, hosting, public domains, verified App Links, or signing until `Game Ready` is recorded in the handoff.

## Required Handoff

At the end of every coding task, update `docs/ai-context/HANDOFF.md` with:

- Date
- Task completed
- Files changed
- Decisions made
- Tests/checks run
- Known blockers
- Next recommended task

## Backend Standards

- Use .NET 10 LTS unless the team explicitly pins another version.
- Use clean architecture boundaries: API, Application, Domain, Infrastructure, Contracts, Tests.
- Business rules belong in Domain/Application, not controllers.
- Use DTOs/contracts for API boundaries.
- Keep database migrations explicit and reviewable.

## Flutter Standards

- Use feature-first structure.
- Use Riverpod for state management unless changed by an architecture decision record.
- Use generated API clients only when contracts are stable.
- Keep cultural copy and design tokens centralized.
- Android is the first production platform. iOS/web remain separate post-launch products unless the team explicitly changes scope.

## Git Standards

- Commit messages should be conventional: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`.
- Each task should end in a coherent commit.
- Do not mix unrelated product areas in one change.

## Safety Rules

- User-generated responses must be reportable.
- Moderation status must be represented in the data model.
- Private groups/family Majlis features must not expose members or comments publicly.
- No cultural, ethnic, sectarian, or regional insults are acceptable as platform behavior.
