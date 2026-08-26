# Plan 005: Privacy-Safe Weekly Leaderboard

## Delivery Order

1. Complete identity age/privacy rules and the immutable XP ledger.
2. Implement the weekly ranking query and deterministic tie behavior.
3. Apply eligibility, moderation, deletion, and block filters before response mapping.
4. Add the Android leaderboard screen and opt-in education/control.
5. Add caching only after privacy invalidation tests pass.

## Validation

- Unit tests for week bounds, ties, and ranking.
- PostgreSQL integration tests for eligibility, duplicate XP, block filtering, rename, and opt-out.
- Widget/integration tests for Arabic/RTL, TalkBack, loading, empty, ineligible, and error states.
