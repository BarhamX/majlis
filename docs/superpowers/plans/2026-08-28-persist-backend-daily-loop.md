# Persisted Backend Daily Loop Implementation Plan

**Goal:** Deliver the complete persisted backend Daily Majlis loop on `feat/persist-daily-attempts`: final attempts, exact XP, UTC published-day streaks, idempotency, history, progress, completed Today state, and spoiler-safe share metadata.

**Scope boundary:** Flutter, share rendering/public deep links, leaderboards, production identity providers, and full content administration/import remain separate milestones within Production V1.

## Global Constraints

- Specs and the API/database contracts are authoritative. Preserve the full Production V1 framing.
- Only an authenticated active user with a completed profile may attempt the challenge in the one Daily Majlis whose status is `published` and whose `PublishDate` is the current UTC date.
- One final attempt per `(UserId, DailyMajlisId)`. The first accepted result is immutable and remains retrievable after the UTC day, correction, or unpublishing.
- Award exactly 10 completion XP plus 5 correctness XP. Attempt, ledger, progress, snapshots, and idempotency record commit in one PostgreSQL transaction.
- Streaks advance across consecutive published UTC content days; a missing publication is exempt, a skipped published day resets to 1, repeated completion is unchanged, and longest streak never decreases.
- Store and replay the original result locale plus the exact post-award lifetime XP/current streak/longest streak snapshots.
- Mutations require UUID `Idempotency-Key`. Same key/payload replays; same key/different payload returns `idempotency_key_reused`; a different key after completion returns `attempt_already_completed` with the existing attempt id.
- New attempts are rejected unless the challenge is today's published challenge and the option belongs to it. Replays and owned reads remain available later.
- Owned result/history/progress/share endpoints never enumerate another user's private state. Share metadata contains no correctness, answer, explanation, identity, XP, or streak.
- Attempt submission uses composed fixed-window account and IP limits of 10/minute and 60/minute, returns RFC 7807 `429 rate_limit_exceeded` with `Retry-After`, and performs no data mutation.
- Use .NET 10, PostgreSQL/EF Core, clean architecture boundaries, explicit forward-only migrations, test-first domain/API work, and a required handoff update.

## Task 1: Add backend CI and prove the identity PostgreSQL baseline

- Add `.github/workflows/backend-ci.yml` for pull requests and pushes to `main` and `feat/**`.
- Use `ubuntu-latest`, read-only repository permissions, `actions/checkout@v6`, `actions/setup-dotnet@v5`, .NET `10.0.x`, and `actions/upload-artifact@v7` for TRX output on failure.
- Restore, build Release, run the three `IdentityProfilePostgreSqlTests`, then run the full backend Release suite. Testcontainers must use the hosted runner's Linux Docker engine.
- Validate workflow syntax and repository conventions, commit, push this task alone, and require the identity job to pass before Task 2.

## Task 2: Clarify normative contracts and traceability before feature code

- Update Spec 001, API contracts, database schema, tasks, and requirement-to-test mapping where the persisted plan is more precise.
- State current-published-only submission, original stored result locale/snapshots, stable `attempt_not_found`, cursor semantics, and the immutable-attempt behavior after content correction/unpublishing.
- Reconcile the stale `UserStreak` checklist wording to the normative `UserProgress` aggregate without adding a second streak authority.
- Do not mark evidence verified before executable tests pass.

## Task 3: Implement domain rules, persistence model, and forward migration test-first

- Add pure tests first for scoring and every streak boundary: first, consecutive published day, skipped published day, missing publication, repeated day, correct/incorrect equality for streaks, and monotonic longest streak.
- Add immutable `UserAttempt`, `XpLedgerEntry`, `UserProgress`, and `IdempotencyRecord` domain/persistence models. Attempts include `ResultLocale`, `LifetimeXpAfter`, `CurrentStreakAfter`, and `LongestStreakAfter`.
- Extend `MajlisDbContext`, EF configurations, and model tests.
- Generate a forward-only migration without editing prior migrations. Add required composite ownership foreign keys, delete behaviors, exact-XP/non-negative checks, unique attempt and ledger constraints, and history/ledger/idempotency indexes.
- Add fresh-database and upgrade-from-current migration tests plus pending-model/script validation.

## Task 4: Implement the transactional daily-loop application and APIs test-first

- Add failing application/PostgreSQL tests for current-day validation, option ownership, exact XP, first/consecutive/reset/exempt streaks, rollback on injected persistence failure, same-key replay, changed-payload rejection, different-key completion conflict, restart durability, and concurrency.
- Implement submission in one EF/PostgreSQL transaction: lock/revalidate the authenticated user, inspect scoped idempotency and request hash, replay or reject, locate prior completed attempt, validate today's published revision/option, then persist attempt, ledger, lazy progress, snapshots, and idempotency together.
- Use database constraints as the final race guard and translate PostgreSQL conflicts to stable API outcomes.
- Add thin controllers/contracts for submit, owned result, newest-first opaque-cursor history (default 20, range 1-50), zero/default progress, and spoiler-safe share metadata.
- Return localized learning content from the immutable stored revision using the attempt's stored result locale.
- Integrate real `hasAttempted`/`attemptId` into Today.

## Task 5: Add rate limiting and complete security/concurrency verification

- Add failing API tests for malformed/missing idempotency keys, completed attempts, unavailable/non-current challenges, option mismatch, cross-user non-enumeration, localization fallback, stable snapshots after later progress/content changes, cursor stability, and spoiler-safe field allowlists.
- Compose fixed-window account and IP partitions at 10/minute and 60/minute for submission. Return `Retry-After` and stable RFC 7807 errors.
- Prove rejected account/IP requests mutate no attempt, ledger, progress, or idempotency data.
- Prove races/restarts leave exactly one attempt, ledger row, and award.

## Task 6: Final documentation, verification, and delivery

- Update Spec 001 task checkboxes only for completed backend work, the requirements-to-tests evidence/status, API/database documentation, MANIFEST if needed, and `docs/ai-context/HANDOFF.md` with the required date/task/files/decisions/tests/blockers/next task.
- Run Release build, targeted identity tests, non-integration tests, complete PostgreSQL integration/full tests, migration validation, docs validation, formatting, and `git diff --check`.
- Request task and whole-branch code review; resolve Critical/Important findings.
- Commit coherently, push `feat/persist-daily-attempts`, open a PR to `main`, and merge only after CI and review are green.

## Assumptions

- Historical/future challenges cannot receive new attempts; accepted results remain retrievable after the content day changes.
- Idempotency records remain replayable for at least 24 hours; cleanup scheduling belongs to production operations.
- Users without accepted attempts have zero progress without a persisted progress row.
- The repository's normative `UserProgress` aggregate fulfills the older `UserStreak` checklist item.
