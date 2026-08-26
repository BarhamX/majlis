# AGENTS.md

This file is the operational contract for AI coding agents working on Majlis.

## Project Summary

Majlis is a Flutter Android mobile game with a .NET backend. It delivers daily Arab cultural challenges, proverbs, stories, discussion prompts, streaks, leaderboards, and shareable cultural cards.

## Read First

Before making changes, read these files in order:

1. `docs/ai-context/PROJECT.md`
2. `docs/ai-context/ARCHITECTURE.md`
3. `docs/ai-context/CONVENTIONS.md`
4. `docs/ai-context/HANDOFF.md`
5. `.specify/memory/constitution.md`
6. The relevant `specs/<feature>/spec.md`, `plan.md`, and `tasks.md`

## Working Rules

- Treat specs as the source of truth.
- Do not invent product behavior that is not in a spec.
- Keep changes small and reviewable.
- Prefer test-first development for backend domain logic and API behavior.
- Preserve cultural respect and authenticity in all copy and content behavior.
- Do not add addictive dark patterns. Retention must be based on ritual, learning, competition, and social belonging.
- Do not implement broad social-media behavior before moderation and reporting exist.
- Do not hardcode daily cultural content into Flutter screens. Daily content must come from backend or seed data.

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
- Android is the MVP platform. iOS/web can be planned later.

## Git Standards

- Commit messages should be conventional: `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`.
- Each task should end in a coherent commit.
- Do not mix unrelated product areas in one change.

## Safety Rules

- User-generated responses must be reportable.
- Moderation status must be represented in the data model.
- Private groups/family Majlis features must not expose members or comments publicly.
- No cultural, ethnic, sectarian, or regional insults are acceptable as platform behavior.
